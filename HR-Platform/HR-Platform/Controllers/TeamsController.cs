using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Manager,HR,Recruiter")]
public class TeamsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeamsController(ApplicationDbContext dbContext)
    {
        this._context = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var teams = await _context.Teams
            .Include(t => t.Manager)
            .ToListAsync();

        return View(teams);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        await PopulateManagersDropdown(team.ManagerId);
        return View(team);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Team team)
    {
        if (id != team.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existingTeam == null)
                {
                    return NotFound();
                }

                existingTeam.Name = team.Name;
                existingTeam.ManagerId = team.ManagerId;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        await PopulateManagersDropdown(team.ManagerId);
        return View(team);
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.TeamMembers)
            .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }




    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateManagersDropdown();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Team team)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        await PopulateManagersDropdown();
        return View(team);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

	[Authorize(Roles = "Manager,Admin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> RemoveMember(int teamId, string userId)
	{
		var teamMember = await _context.TeamMembers
			.FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

		if (teamMember == null)
		{
			TempData["ErrorMessage"] = "Team member not found.";
			return RedirectToAction("Details", new { id = teamId });
		}

		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
		if (user == null)
		{
			TempData["ErrorMessage"] = "User not found.";
			return RedirectToAction("Details", new { id = teamId });
		}

		_context.TeamMembers.Remove(teamMember);

		var employeeRoleId = await _context.Roles
			.Where(r => r.Name == "Employee")
			.Select(r => r.Id)
			.FirstOrDefaultAsync();

		if (await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == employeeRoleId))
		{
			var userRole = await _context.UserRoles
				.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == employeeRoleId);

			if (userRole != null)
			{
				_context.UserRoles.Remove(userRole);
			}
		}

		var userRoleId = await _context.Roles
			.Where(r => r.Name == "User")
			.Select(r => r.Id)
			.FirstOrDefaultAsync();

		if (userRoleId != null && !await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == userRoleId))
		{
			_context.UserRoles.Add(new IdentityUserRole<string>
			{
				UserId = userId,
				RoleId = userRoleId
			});
		}

		await _context.SaveChangesAsync();

		TempData["SuccessMessage"] = "Member removed and reverted to User role successfully.";
		return RedirectToAction("Details", new { id = teamId });
	}

	private async Task PopulateManagersDropdown(string? selectedManagerId = null)
    {
        var managerRoleId = await _context.Roles
            .Where(r => r.Name == "Manager")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var managers = await _context.UserRoles
            .Where(ur => ur.RoleId == managerRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var managerList = _context.Users.Where(u => managers.Contains(u.Id)).ToList();

        ViewData["Managers"] = new SelectList(managerList, "Id", "UserName", selectedManagerId);
    }
}