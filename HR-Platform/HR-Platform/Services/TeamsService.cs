using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class TeamsService
{
    private readonly ApplicationDbContext _context;

    public TeamsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await _context.Teams.Include(t => t.Manager).ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        return await _context.Teams.FindAsync(id);
    }

    public async Task<Team?> GetTeamDetailsAsync(int id)
    {
        return await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.TeamMembers)
            .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> UpdateTeamAsync(Team team)
    {
        var existingTeam = await _context.Teams.FindAsync(team.Id);
        if (existingTeam == null)
        {
            return false;
        }

        existingTeam.Name = team.Name;
        existingTeam.ManagerId = team.ManagerId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateTeamAsync(Team team)
    {
        try
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return false;
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveTeamMemberAsync(int teamId, string userId)
    {
        var teamMember = await _context.TeamMembers.FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        if (teamMember == null)
        {
            return false;
        }

        _context.TeamMembers.Remove(teamMember);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task PopulateManagersDropdown(ViewDataDictionary viewData, string? selectedManagerId = null)
    {
        var managerRoleId = await _context.Roles
            .Where(r => r.Name == "Manager")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var managers = await _context.UserRoles
            .Where(ur => ur.RoleId == managerRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var managerList = await _context.Users
            .Where(u => managers.Contains(u.Id))
            .ToListAsync();

        viewData["Managers"] = new SelectList(managerList, "Id", "UserName", selectedManagerId);
    }
}