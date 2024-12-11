using Microsoft.EntityFrameworkCore;

public class LeaveRequestsServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly LeaveRequestsService _service;

    public LeaveRequestsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "LeaveRequestsTestDb")
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new LeaveRequestsService(_context);
        ClearDatabase();
    }

    public void ClearDatabase()
    {
        _context.LeaveRequests.RemoveRange(_context.LeaveRequests);
        _context.TeamMembers.RemoveRange(_context.TeamMembers);
        _context.Teams.RemoveRange(_context.Teams);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetUserLeaveRequestsAsync_ReturnsUpcomingLeaveRequests_ForValidUserId()
    {
        var userId = "user1";
        var today = DateTime.UtcNow;

        var leaveRequest1 = new LeaveRequest
        {
            EmployeeId = userId,
            StartDate = today.AddDays(1),
            EndDate = today.AddDays(5),
            TeamId = 1,
            Status = "Pending"
        };
        var leaveRequest2 = new LeaveRequest
        {
            EmployeeId = userId,
            StartDate = today.AddDays(6),
            EndDate = today.AddDays(10),
            TeamId = 1,
            Status = "Approved"
        };
        var pastLeaveRequest = new LeaveRequest
        {
            EmployeeId = userId,
            StartDate = today.AddDays(-10),
            EndDate = today.AddDays(-5),
            TeamId = 1,
            Status = "Approved"
        };

        _context.LeaveRequests.AddRange(leaveRequest1, leaveRequest2, pastLeaveRequest);
        await _context.SaveChangesAsync();

        var result = await _service.GetUserLeaveRequestsAsync(userId, today);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(today.AddDays(1), result[0].StartDate);
        Assert.Equal(today.AddDays(6), result[1].StartDate);
    }

    [Fact]
    public async Task HasOverlappingLeaveRequestAsync_ReturnsTrue_ForOverlappingDates()
    {
        var userId = "user1";
        var existingLeaveRequest = new LeaveRequest
        {
            EmployeeId = userId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            TeamId = 1,
            Status = "Approved"
        };

        _context.LeaveRequests.Add(existingLeaveRequest);
        await _context.SaveChangesAsync();

        var newStartDate = DateTime.UtcNow.AddDays(3);
        var newEndDate = DateTime.UtcNow.AddDays(6);

        var result = await _service.HasOverlappingLeaveRequestAsync(userId, newStartDate, newEndDate);

        Assert.True(result);
    }

    [Fact]
    public async Task GetTeamMemberAsync_ReturnsTeamMember_ForValidUserId()
    {
        var userId = "user1";
        var team = new Team { Id = 1, Name = "Development Team", ManagerId = "manager1" };
        var teamMember = new TeamMember
        {
            UserId = userId,
            TeamId = team.Id,
            Team = team,
            JoinedAt = DateTime.UtcNow.AddMonths(-3)
        };

        _context.Teams.Add(team);
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        var result = await _service.GetTeamMemberAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(team.Id, result.TeamId);
        Assert.Equal("Development Team", result.Team.Name);
    }

    [Fact]
    public async Task SubmitLeaveRequestAsync_AddsLeaveRequest()
    {
        var userId = "user1";
        var teamId = 1;
        var managerId = "manager1";
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(10);

        var team = new Team { Id = teamId, Name = "Development Team", ManagerId = managerId };
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        await _service.SubmitLeaveRequestAsync(userId, teamId, startDate, endDate, managerId);

        var leaveRequests = await _context.LeaveRequests.ToListAsync();
        Assert.Single(leaveRequests);
        Assert.Equal(userId, leaveRequests[0].EmployeeId);
        Assert.Equal(teamId, leaveRequests[0].TeamId);
        Assert.Equal(startDate, leaveRequests[0].StartDate);
        Assert.Equal(endDate, leaveRequests[0].EndDate);
        Assert.Equal("Pending", leaveRequests[0].Status);
        Assert.Equal(managerId, leaveRequests[0].ManagerId);
    }

    [Fact]
    public async Task GetLeaveRequestByIdAsync_ReturnsLeaveRequest_ForValidId()
    {
        ClearDatabase();

        var leaveRequest = new LeaveRequest
        {
            Id = 1,
            EmployeeId = "user1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = "Pending",
            TeamId = 1
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        var result = await _service.GetLeaveRequestByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("user1", result.EmployeeId);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task GetLeaveRequestByIdAsync_ReturnsNull_ForInvalidId()
    {
        ClearDatabase();

        var result = await _service.GetLeaveRequestByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateLeaveRequestStatusAsync_UpdatesStatus_ForValidId()
    {
        ClearDatabase();

        var leaveRequest = new LeaveRequest
        {
            Id = 1,
            EmployeeId = "user1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = "Pending",
            TeamId = 1
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        await _service.UpdateLeaveRequestStatusAsync(1, "Approved");

        var updatedLeaveRequest = await _context.LeaveRequests.FindAsync(1);

        Assert.NotNull(updatedLeaveRequest);
        Assert.Equal("Approved", updatedLeaveRequest.Status);
    }

    [Fact]
    public async Task UpdateLeaveRequestStatusAsync_ThrowsException_ForInvalidId()
    {
        ClearDatabase();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateLeaveRequestStatusAsync(99, "Approved")
        );

        Assert.Equal("Leave request not found.", exception.Message);
    }
}