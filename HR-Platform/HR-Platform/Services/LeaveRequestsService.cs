using Microsoft.EntityFrameworkCore;

public class LeaveRequestsService
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveRequest>> GetUserLeaveRequestsAsync(string userId, DateTime today)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == userId && lr.EndDate >= today)
            .OrderBy(lr => lr.StartDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingLeaveRequestAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == userId && lr.StartDate <= endDate && lr.EndDate >= startDate)
            .AnyAsync();
    }

    public async Task<TeamMember> GetTeamMemberAsync(string userId)
    {
        return await _context.TeamMembers
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.UserId == userId);
    }

    public async Task SubmitLeaveRequestAsync(string userId, int teamId, DateTime startDate, DateTime endDate, string managerId)
    {
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = userId,
            TeamId = teamId,
            StartDate = startDate,
            EndDate = endDate,
            Status = "Pending",
            ManagerId = managerId
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestsForReviewAsync(string? managerId, bool isAdmin)
    {
        var query = _context.LeaveRequests.AsQueryable();

        if (!isAdmin)
        {
            if (string.IsNullOrEmpty(managerId))
            {
                return new List<LeaveRequest>();
            }

            query = query.Where(lr => lr.ManagerId == managerId);
        }

        query = query
            .Include(lr => lr.Employee)
            .Include(lr => lr.Team);

        return await query.OrderByDescending(lr => lr.StartDate).ToListAsync();
    }

    public async Task<LeaveRequest> GetLeaveRequestByIdAsync(int id)
    {
        return await _context.LeaveRequests.FindAsync(id);
    }

    public async Task UpdateLeaveRequestStatusAsync(int id, string status)
    {
        var leaveRequest = await GetLeaveRequestByIdAsync(id);

        if (leaveRequest == null)
        {
            throw new KeyNotFoundException("Leave request not found.");
        }

        leaveRequest.Status = status;
        await _context.SaveChangesAsync();
    }
}