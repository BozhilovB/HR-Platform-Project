using Microsoft.EntityFrameworkCore;

public class TeamsServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly TeamsService _service;

    public TeamsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TeamsServiceTestDb")
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TeamsService(_context);
        ClearDatabase();
    }

    private void ClearDatabase()
    {
        _context.Teams.RemoveRange(_context.Teams);
        _context.TeamMembers.RemoveRange(_context.TeamMembers);
        _context.Users.RemoveRange(_context.Users);
        _context.Roles.RemoveRange(_context.Roles);
        _context.UserRoles.RemoveRange(_context.UserRoles);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllTeamsAsync_ReturnsAllTeamsWithManagers()
    {
        var manager = new ApplicationUser
        {
            Id = "manager1",
            UserName = "ManagerOne",
            FirstName = "John",
            LastName = "Doe"
        };

        var team1 = new Team
        {
            Id = 1,
            Name = "Development Team",
            ManagerId = "manager1",
            Manager = manager
        };

        var team2 = new Team
        {
            Id = 2,
            Name = "Design Team",
            ManagerId = "manager1",
            Manager = manager
        };

        _context.Users.Add(manager);
        _context.Teams.AddRange(team1, team2);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllTeamsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "Development Team");
        Assert.Contains(result, t => t.Name == "Design Team");
        Assert.All(result, t => Assert.Equal("ManagerOne", t.Manager.UserName));
    }

    [Fact]
    public async Task GetTeamByIdAsync_ReturnsTeam_ForValidId()
    {
        var manager = new ApplicationUser
        {
            Id = "manager1",
            UserName = "Manager One",
            FirstName = "John",
            LastName = "Doe"
        };

        var team = new Team
        {
            Id = 1,
            Name = "Development Team",
            ManagerId = "manager1",
            Manager = manager
        };

        _context.Users.Add(manager);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        var result = await _service.GetTeamByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Development Team", result.Name);
        Assert.Equal("manager1", result.ManagerId);
    }

    [Fact]
    public async Task GetTeamByIdAsync_ReturnsNull_ForInvalidId()
    {
        var result = await _service.GetTeamByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTeamDetailsAsync_ReturnsDetailedTeam_ForValidId()
    {
        var manager = new ApplicationUser
        {
            Id = "manager1",
            UserName = "Manager One",
            FirstName = "John",
            LastName = "Doe"
        };

        var user = new ApplicationUser
        {
            Id = "user1",
            UserName = "User One",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var team = new Team
        {
            Id = 1,
            Name = "Development Team",
            ManagerId = "manager1",
            Manager = manager
        };

        var teamMember = new TeamMember
        {
            UserId = "user1",
            TeamId = 1,
            Team = team,
            User = user,
            JoinedAt = DateTime.UtcNow.AddMonths(-3)
        };

        _context.Users.AddRange(manager, user);
        _context.Teams.Add(team);
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        var result = await _service.GetTeamDetailsAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Development Team", result.Name);
        Assert.Equal("manager1", result.ManagerId);
        Assert.Equal(1, result.TeamMembers.Count);
        Assert.Contains(result.TeamMembers, tm => tm.UserId == "user1");
    }

    [Fact]
    public async Task GetTeamDetailsAsync_ReturnsNull_ForInvalidId()
    {
        var result = await _service.GetTeamDetailsAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTeamAsync_UpdatesTeam_ForValidId()
    {
        var manager = new ApplicationUser
        {
            Id = "manager1",
            UserName = "Manager One",
            FirstName = "John",
            LastName = "Doe"
        };

        var team = new Team
        {
            Id = 1,
            Name = "Development Team",
            ManagerId = "manager1",
            Manager = manager
        };

        _context.Users.Add(manager);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var updatedTeam = new Team
        {
            Id = 1,
            Name = "Updated Development Team",
            ManagerId = "manager1"
        };

        var result = await _service.UpdateTeamAsync(updatedTeam);

        Assert.True(result);
        var teamInDb = await _context.Teams.FindAsync(1);
        Assert.NotNull(teamInDb);
        Assert.Equal("Updated Development Team", teamInDb.Name);
        Assert.Equal("manager1", teamInDb.ManagerId);
    }

    [Fact]
    public async Task UpdateTeamAsync_ReturnsFalse_ForInvalidId()
    {
        var updatedTeam = new Team
        {
            Id = 999,
            Name = "Non-Existent Team",
            ManagerId = "manager1"
        };

        var result = await _service.UpdateTeamAsync(updatedTeam);
        Assert.False(result);
    }

    [Fact]
    public async Task CreateTeamAsync_CreatesNewTeam()
    {
        var team = new Team
        {
            Name = "New Team",
            ManagerId = "manager1"
        };

        var manager = new ApplicationUser
        {
            Id = "manager1",
            UserName = "Manager One",
            FirstName = "John",
            LastName = "Doe"
        };

        _context.Users.Add(manager);
        await _context.SaveChangesAsync();
        var result = await _service.CreateTeamAsync(team);

        Assert.True(result);
        var teamInDb = await _context.Teams.FirstOrDefaultAsync(t => t.Name == "New Team");
        Assert.NotNull(teamInDb);
        Assert.Equal("New Team", teamInDb.Name);
        Assert.Equal("manager1", teamInDb.ManagerId);
    }

    [Fact]
    public async Task DeleteTeamAsync_DeletesTeam_ForValidId()
    {
        var team = new Team
        {
            Id = 1,
            Name = "Team to Delete",
            ManagerId = "manager1"
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        var result = await _service.DeleteTeamAsync(1);

        Assert.True(result);
        var teamInDb = await _context.Teams.FindAsync(1);
        Assert.Null(teamInDb);
    }

    [Fact]
    public async Task DeleteTeamAsync_ReturnsFalse_ForInvalidId()
    {
        var result = await _service.DeleteTeamAsync(999);
        Assert.False(result);
    }
}