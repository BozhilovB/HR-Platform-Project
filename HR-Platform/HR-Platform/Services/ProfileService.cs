using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class ProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ProfileService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        return await _userManager.GetUserAsync(user);
    }

    public async Task<ApplicationUser?> GetTargetUserAsync(string userId)
    {
        return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<string>> GetCurrentUserRolesAsync(ApplicationUser currentUser)
    {
        return (await _userManager.GetRolesAsync(currentUser)).ToList();
    }

    public async Task<List<Team>> GetManagedTeamsAsync(string userId)
    {
        return await _context.Teams.Where(t => t.ManagerId == userId).ToListAsync();
    }

    public async Task<List<TeamMember>> GetTeamMembershipsAsync(string userId)
    {
        return await _context.TeamMembers
            .Where(tm => tm.UserId == userId)
            .Include(tm => tm.Team)
            .ToListAsync();
    }

    public async Task<bool> IsManagerOfUserAsync(string managerId, string targetUserId)
    {
        return await _context.Teams.AnyAsync(t => t.ManagerId == managerId && t.TeamMembers.Any(tm => tm.UserId == targetUserId));
    }

    public async Task UpdateSalaryAsync(string userId, decimal salary)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.Salary = salary;
            await _context.SaveChangesAsync();
        }
    }
}