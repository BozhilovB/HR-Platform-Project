using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using Moq;
using System.Security.Claims;

public class ProfileServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationDbContext _context;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "ProfileServiceTestDb")
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null, null, null, null, null, null, null, null);

        _service = new ProfileService(_userManagerMock.Object, _context);
        ClearDatabase();
    }

    private void ClearDatabase()
    {
        _context.TeamMembers.RemoveRange(_context.TeamMembers);
        _context.Teams.RemoveRange(_context.Teams);
        _context.LeaveRequests.RemoveRange(_context.LeaveRequests);
        _context.Users.RemoveRange(_context.Users);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsCurrentUser_ForValidClaimsPrincipal()
    {
        var userId = "user1";
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        _userManagerMock.Setup(m => m.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);
        var result = await _service.GetCurrentUserAsync(claimsPrincipal);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
    }

    [Fact]
    public async Task GetTargetUserAsync_ReturnsUser_ForValidUserId()
    {
        var userId = "user1";
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Salary = 50000
        };

        var users = new List<ApplicationUser> { user }.AsQueryable().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);

        var result = await _service.GetTargetUserAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
    }

    [Fact]
    public async Task GetCurrentUserRolesAsync_ReturnsRoles()
    {
        var userId = "user1";
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var roles = new List<string> { "Admin", "Manager" };

        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);

        var result = await _service.GetCurrentUserRolesAsync(user);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("Admin", result);
        Assert.Contains("Manager", result);
    }

    [Fact]
    public async Task GetManagedTeamsAsync_ReturnsTeamsForManager()
    {
        var managerId = "manager1";

        var team1 = new Team { Id = 1, Name = "Development Team", ManagerId = managerId };
        var team2 = new Team { Id = 2, Name = "Design Team", ManagerId = managerId };
        var unrelatedTeam = new Team { Id = 3, Name = "Sales Team", ManagerId = "anotherManager" };

        _context.Teams.AddRange(team1, team2, unrelatedTeam);
        await _context.SaveChangesAsync();

        var result = await _service.GetManagedTeamsAsync(managerId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "Development Team");
        Assert.Contains(result, t => t.Name == "Design Team");
    }

    [Fact]
    public async Task GetTeamMembershipsAsync_ReturnsTeamsForUser()
    {
        var userId = "user1";

        var team1 = new Team { Id = 1, Name = "Development Team", ManagerId = "manager1" };
        var team2 = new Team { Id = 2, Name = "Design Team", ManagerId = "manager2" };

        var membership1 = new TeamMember { UserId = userId, TeamId = team1.Id, Team = team1, JoinedAt = DateTime.UtcNow.AddMonths(-3) };
        var membership2 = new TeamMember { UserId = userId, TeamId = team2.Id, Team = team2, JoinedAt = DateTime.UtcNow.AddMonths(-1) };

        _context.Teams.AddRange(team1, team2);
        _context.TeamMembers.AddRange(membership1, membership2);
        await _context.SaveChangesAsync();

        var result = await _service.GetTeamMembershipsAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, tm => tm.Team.Name == "Development Team");
        Assert.Contains(result, tm => tm.Team.Name == "Design Team");
    }

    [Fact]
    public async Task IsManagerOfUserAsync_ReturnsTrue_WhenManagerOfUser()
    {
        var managerId = "manager1";
        var userId = "user1";

        var team = new Team { Id = 1, Name = "Development Team", ManagerId = managerId };
        var teamMember = new TeamMember { UserId = userId, TeamId = team.Id, Team = team };

        _context.Teams.Add(team);
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        var result = await _service.IsManagerOfUserAsync(managerId, userId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsManagerOfUserAsync_ReturnsFalse_WhenNotManagerOfUser()
    {
        var managerId = "manager1";
        var userId = "user1";

        var team = new Team { Id = 1, Name = "Development Team", ManagerId = "otherManager" };
        var teamMember = new TeamMember { UserId = userId, TeamId = team.Id, Team = team };

        _context.Teams.Add(team);
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        var result = await _service.IsManagerOfUserAsync(managerId, userId);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateSalaryAsync_UpdatesSalary_ForValidUser()
    {
        var userId = "user1";
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Salary = 50000
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var newSalary = 75000;
        await _service.UpdateSalaryAsync(userId, newSalary);

        var updatedUser = await _context.Users.FindAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal(newSalary, updatedUser.Salary);
    }

    [Fact]
    public async Task UpdateSalaryAsync_DoesNothing_ForInvalidUser()
    {
        var userId = "nonexistentUser";
        var newSalary = 75000;

        await _service.UpdateSalaryAsync(userId, newSalary);

        var result = await _context.Users.FindAsync(userId);
        Assert.Null(result);
    }
}