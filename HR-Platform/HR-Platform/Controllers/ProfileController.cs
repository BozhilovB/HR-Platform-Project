using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ProfileController : Controller
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<IActionResult> Index(string? id = null)
    {
        var currentUser = await _profileService.GetCurrentUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var userId = id ?? currentUser.Id;
        var targetUser = await _profileService.GetTargetUserAsync(userId);
        if (targetUser == null)
        {
            return NotFound();
        }

        var currentUserRoles = await _profileService.GetCurrentUserRolesAsync(currentUser);
        if (!currentUserRoles.Contains("Admin") &&
            !currentUserRoles.Contains("HR") &&
            !currentUserRoles.Contains("Manager") &&
            targetUser.Id != currentUser.Id)
        {
            return Forbid();
        }

        var managedTeams = await _profileService.GetManagedTeamsAsync(targetUser.Id);
        var teamMemberships = await _profileService.GetTeamMembershipsAsync(targetUser.Id);

        var isOwnProfile = targetUser.Id == currentUser.Id;

        return View(new ProfileViewModel
        {
            User = targetUser,
            ManagedTeams = managedTeams,
            MemberTeams = teamMemberships,
            IsOwnProfile = isOwnProfile,
            Salary = targetUser.Salary
        });
    }

    [Authorize(Roles = "HR,Admin,Recruiter,Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSalary(string userId, decimal salary)
    {
        var currentUser = await _profileService.GetCurrentUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var userToUpdate = await _profileService.GetTargetUserAsync(userId);
        if (userToUpdate == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Manager"))
        {
            var managesTeam = await _profileService.IsManagerOfUserAsync(currentUser.Id, userId);
            if (!managesTeam)
            {
                return Forbid();
            }
        }

        await _profileService.UpdateSalaryAsync(userId, salary);

        return RedirectToAction("Index", new { id = userId });
    }
}