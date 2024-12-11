using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

public class AdminServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationDbContext _context;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);

        _adminService = new AdminService(_context, _userManagerMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var roles = new List<IdentityRole>
        {
            new IdentityRole { Id = "1", Name = "Admin" },
            new IdentityRole { Id = "2", Name = "User" }
        };
        _context.Roles.AddRange(roles);

        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };
        _context.Users.AddRange(users);

        var teams = new List<Team>
        {
            new Team { Id = 1, Name = "Team A", ManagerId = "1" },
            new Team { Id = 2, Name = "Team B", ManagerId = "2" }
        };
        _context.Teams.AddRange(teams);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers_WhenNoFilterIsApplied()
    {
        _userManagerMock.Setup(um => um.Users).Returns(_context.Users.AsQueryable());

        var result = await _adminService.GetUsersAsync(null, null, null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUsersAsync_FiltersBySearchTerm()
    {
        _userManagerMock.Setup(um => um.Users).Returns(_context.Users.AsQueryable());

        var result = await _adminService.GetUsersAsync("John", null, null);

        Assert.Single(result);
        Assert.Equal("john@example.com", result[0].Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsCorrectUser()
    {
        _userManagerMock.Setup(um => um.Users).Returns(_context.Users.AsQueryable());

        var result = await _adminService.GetUserByIdAsync("1");

        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsAllRoles()
    {
        var result = await _adminService.GetAllRolesAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Text == "Admin");
        Assert.Contains(result, r => r.Text == "User");
    }

    [Fact]
    public async Task GetAllTeamsAsync_ReturnsCorrectTeamSelection()
    {
        var userId = "1";
        _context.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = 1 });
        _context.SaveChanges();

        var result = await _adminService.GetAllTeamsAsync(userId);

        Assert.Equal(2, result.Count);
        Assert.True(result.First(t => t.Value == "1").Selected);
        Assert.False(result.First(t => t.Value == "2").Selected);
    }

    [Fact]
    public async Task GetUserRolesAsync_ReturnsCorrectRoles()
    {
        var user = _context.Users.First();
        var roles = new List<string> { "Admin", "User" };

        _userManagerMock.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var result = await _adminService.GetUserRolesAsync(user);

        Assert.Equal(roles.Count, result.Count);
        Assert.Contains("Admin", result);
        Assert.Contains("User", result);
    }

    [Fact]
    public async Task GetUserRolesAsync_ReturnsEmptyList_WhenUserHasNoRoles()
    {
        var user = _context.Users.First();

        _userManagerMock.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var result = await _adminService.GetUserRolesAsync(user);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUserDetailsAndRoles()
    {
        var user = _context.Users.First();
        var model = new UserEditViewModel
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "updatedemail@example.com",
            SelectedRoles = new List<string> { "Admin" },
            SelectedTeamIds = new List<string> { "1" }
        };

        var currentRoles = new List<string> { "User" };

        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(currentRoles);
        _userManagerMock.Setup(um => um.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _adminService.UpdateUserAsync(user, model);

        Assert.Equal(model.FirstName, user.FirstName);
        Assert.Equal(model.LastName, user.LastName);
        Assert.Equal(model.Email, user.Email);
        _userManagerMock.Verify(um => um.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin"))), Times.Once);
        _userManagerMock.Verify(um => um.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("User"))), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_HandlesTeamChanges()
    {
        var user = _context.Users.Include(u => u.Teams).First();
        var model = new UserEditViewModel
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "updatedemail@example.com",
            SelectedRoles = new List<string> { "Admin" },
            SelectedTeamIds = new List<string> { "1" }
        };

        var currentRoles = new List<string> { "User" };

        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(currentRoles);
        _userManagerMock.Setup(um => um.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        _context.TeamMembers.Add(new TeamMember { UserId = user.Id, TeamId = 2 });
        _context.SaveChanges();

        await _adminService.UpdateUserAsync(user, model);

        var teamMembers = _context.TeamMembers.Where(tm => tm.UserId == user.Id).ToList();
        Assert.Single(teamMembers);
        Assert.Equal(1, teamMembers.First().TeamId);
    }
}