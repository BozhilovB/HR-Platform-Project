using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index(string? id = null)
    {
        var userId = id ?? _userManager.GetUserId(User);
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        var targetUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (targetUser == null)
        {
            return NotFound();
        }

        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        if (!currentUserRoles.Contains("Admin") &&
            !currentUserRoles.Contains("HR") &&
            !currentUserRoles.Contains("Manager") &&
            targetUser.Id != currentUser.Id)
        {
            return Forbid();
        }

        var managedTeam = await _context.Teams
            .Where(t => t.ManagerId == targetUser.Id)
            .FirstOrDefaultAsync();

        var teamMemberships = await _context.TeamMembers
            .Where(tm => tm.UserId == targetUser.Id)
            .Include(tm => tm.Team)
            .ToListAsync();

        var isOwnProfile = targetUser.Id == currentUser.Id;

        return View(new ProfileViewModel
        {
            User = targetUser,
            ManagedTeam = managedTeam,
            MemberTeams = teamMemberships,
            IsOwnProfile = isOwnProfile
        });
    }
}
