using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<ApplicationUser>> GetUsersAsync(string? searchTerm, string? team, string? role)
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

        return await usersQuery.ToListAsync();
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string id)
    {
        return await _userManager.Users.Include(u => u.Teams).FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<SelectListItem>> GetAllRolesAsync()
    {
        var roles = await _context.Roles.ToListAsync();
        return roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();
    }

    public async Task<List<SelectListItem>> GetAllTeamsAsync(string userId)
    {
        var allTeams = await _context.Teams.ToListAsync();
        var userTeams = await _context.TeamMembers
            .Where(tm => tm.UserId == userId)
            .Select(tm => tm.TeamId)
            .ToListAsync();

        return allTeams.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.Name,
            Selected = userTeams.Contains(t.Id)
        }).ToList();
    }

    public async Task<List<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task UpdateUserAsync(ApplicationUser user, UserEditViewModel model)
{
    user.FirstName = model.FirstName.Trim();
    user.LastName = model.LastName.Trim();

    if (user.Email != model.Email)
    {
        user.Email = model.Email;
        user.UserName = model.Email;
    }

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

    var jobApplications = await _context.JobApplications
        .Where(ja => ja.ApplicantEmail == user.Email)
        .ToListAsync();

    foreach (var jobApplication in jobApplications)
    {
        jobApplication.ApplicantName = $"{user.FirstName} {user.LastName}";
        jobApplication.ApplicantEmail = user.Email;
    }

    var leaveRequests = await _context.LeaveRequests
        .Where(lr => lr.EmployeeId == user.Id)
        .ToListAsync();

    foreach (var leaveRequest in leaveRequests)
    {
        leaveRequest.EmployeeId = user.Id;
        leaveRequest.Employee = user;
    }

    await _userManager.UpdateAsync(user);
    await _context.SaveChangesAsync();
}


    public async Task DeleteUserAsync(ApplicationUser user)
    {
        var applicantEmail = user.Email;

        _context.TeamMembers.RemoveRange(_context.TeamMembers.Where(tm => tm.UserId == user.Id));
        _context.JobApplications.RemoveRange(_context.JobApplications.Where(ja => ja.ApplicantEmail == applicantEmail));

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await _context.SaveChangesAsync();
    }
}