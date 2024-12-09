using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? team, string? role)
    {
        var usersQuery = _userManager.Users.Include(u => u.Teams).ThenInclude(tm => tm.Team).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedTerm = searchTerm.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.FirstName.ToLower().Contains(normalizedTerm) ||
                u.LastName.ToLower().Contains(normalizedTerm) ||
                u.Email.ToLower().Contains(normalizedTerm) ||
                (u.FirstName + " " + u.LastName).ToLower().Contains(normalizedTerm));
        }

        if (!string.IsNullOrWhiteSpace(team))
        {
            var normalizedTeam = team.Trim();
            usersQuery = usersQuery.Where(u =>
                u.Teams.Any(tm => tm.Team.Name.Contains(normalizedTeam)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = role.Trim();
            var userIdsWithRole = (await _context.UserRoles
                .Where(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == normalizedRole))
                .Select(ur => ur.UserId)
                .ToListAsync());

            usersQuery = usersQuery.Where(u => userIdsWithRole.Contains(u.Id));
        }

        var users = await usersQuery.ToListAsync();
        ViewData["SearchTerm"] = searchTerm;
        ViewData["TeamFilter"] = team;
        ViewData["RoleFilter"] = role;

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.Users.Include(u => u.Teams).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var allRoles = await _context.Roles.ToListAsync();
        var allTeams = await _context.Teams.ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);

        var viewModel = new UserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            SelectedRoles = userRoles.ToList(),
            SelectedTeamIds = user.Teams.Select(t => t.TeamId.ToString()).ToList(),
            Roles = allRoles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = userRoles.Contains(r.Name)
            }).ToList(),
            Teams = allTeams.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Selected = user.Teams.Any(ut => ut.TeamId == t.Id)
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid data.";
            return RedirectToAction("Index");
        }
        
        model.FirstName = model.FirstName?.Trim();
        model.LastName = model.LastName?.Trim();

        if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
        {
            TempData["ErrorMessage"] = "First and last names cannot be empty.";
            return RedirectToAction("EditUser", new { id = model.Id });
        }

        if (model.FirstName.Split(' ').Length > 1 || model.LastName.Split(' ').Length > 1)
        {
            TempData["ErrorMessage"] = "Names should only consist of a single word each.";
            return RedirectToAction("EditUser", new { id = model.Id });
        }

        var user = await _userManager.Users.Include(u => u.Teams).FirstOrDefaultAsync(u => u.Id == model.Id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

        if (rolesToAdd.Any())
            await _userManager.AddToRolesAsync(user, rolesToAdd);

        if (rolesToRemove.Any())
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        var currentTeamIds = user.Teams.Select(t => t.TeamId.ToString()).ToList();
        var teamsToAdd = model.SelectedTeamIds.Except(currentTeamIds).Select(int.Parse).ToList();
        var teamsToRemove = currentTeamIds.Except(model.SelectedTeamIds).Select(int.Parse).ToList();

        foreach (var teamId in teamsToAdd)
        {
            _context.TeamMembers.Add(new TeamMember { UserId = user.Id, TeamId = teamId, JoinedAt = DateTime.UtcNow });
        }

        foreach (var teamId in teamsToRemove)
        {
            var teamMember = await _context.TeamMembers.FirstOrDefaultAsync(tm => tm.UserId == user.Id && tm.TeamId == teamId);
            if (teamMember != null)
                _context.TeamMembers.Remove(teamMember);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        _context.TeamMembers.RemoveRange(_context.TeamMembers.Where(tm => tm.UserId == id));
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded) TempData["SuccessMessage"] = "User deleted successfully.";
        else TempData["ErrorMessage"] = "Failed to delete the user.";

        return RedirectToAction("Index");
    }
}