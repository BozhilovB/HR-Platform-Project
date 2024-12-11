using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

public class JobApplicationsServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationDbContext _context;
    private readonly JobApplicationsService _service;

    public JobApplicationsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);

        _service = new JobApplicationsService(_context, _userManagerMock.Object);
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Teams.Add(new Team { Id = 1, Name = "Team A", ManagerId = "1" });
        _context.JobPostings.Add(new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            RecruiterId = "1",
            Description = "Develop software applications",
            PostedDate = DateTime.UtcNow
        });
        _context.Users.Add(new ApplicationUser
        {
            Id = "1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Salary = 50000
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetApplicantsAsync_ReturnsApplicants_ForValidJobPostingId()
    {
        _context.JobApplications.Add(new JobApplication
        {
            JobPostingId = 1,
            Status = "Pending",
            ApplicantName = "Jane Doe",
            ApplicantEmail = "jane@example.com",
            ResumeUrl = "http://example.com/resume.pdf"
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetApplicantsAsync(1);

        Assert.Single(result);
        Assert.Equal("Pending", result.First().Status);
    }

    [Fact]
    public async Task ApplyForJobAsync_CreatesNewJobApplication()
    {
        var user = new ApplicationUser
        {
            Id = "1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser != null)
        {
            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(um => um.FindByEmailAsync(user.Email)).ReturnsAsync(user);

        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer"
        };

        var existingJobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == jobPosting.Id);
        if (existingJobPosting == null)
        {
            _context.JobPostings.Add(jobPosting);
            await _context.SaveChangesAsync();
        }

        var model = new ApplyJobViewModel
        {
            JobPostingId = 1,
            ResumeUrl = "http://example.com/resume.pdf"
        };

        await _service.ApplyForJobAsync(model, user.Email);

        var application = await _context.JobApplications.FirstOrDefaultAsync();

        Assert.NotNull(application);
        Assert.Equal("Pending", application.Status);
        Assert.Equal("John Doe", application.ApplicantName);
        Assert.Equal("john@example.com", application.ApplicantEmail);
        Assert.Equal(1, application.JobPostingId);
    }

    [Fact]
    public async Task ApproveApplicationAsync_ApprovesApplicationAndUpdatesUser()
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == "1");
        if (existingUser != null)
        {
            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();
        }

        var existingJobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == 1);
        if (existingJobPosting != null)
        {
            _context.JobPostings.Remove(existingJobPosting);
            await _context.SaveChangesAsync();
        }

        var existingApplication = await _context.JobApplications.FirstOrDefaultAsync(ja => ja.Id == 1);
        if (existingApplication != null)
        {
            _context.JobApplications.Remove(existingApplication);
            await _context.SaveChangesAsync();
        }

        var user = new ApplicationUser
        {
            Id = "1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _context.Users.Add(user);

        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            Description = "Develop and maintain software applications.",
            RecruiterId = "1",
            PostedDate = DateTime.UtcNow
        };

        _context.JobPostings.Add(jobPosting);

        var jobApplication = new JobApplication
        {
            Id = 1,
            JobPostingId = 1,
            ApplicantEmail = "john@example.com",
            ApplicantName = "John Doe",
            Status = "Pending",
            ResumeUrl = "http://example.com/resume.pdf"
        };

        _context.JobApplications.Add(jobApplication);

        await _context.SaveChangesAsync();

        var model = new ApproveApplicationViewModel
        {
            ApplicationId = 1,
            ApplicantEmail = "john@example.com",
            Salary = 60000,
            SelectedTeamId = 1
        };

        _userManagerMock.Setup(um => um.IsInRoleAsync(user, "User")).ReturnsAsync(true);
        _userManagerMock.Setup(um => um.RemoveFromRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRoleAsync(user, "Employee")).ReturnsAsync(IdentityResult.Success);

        await _service.ApproveApplicationAsync(model);

        var updatedApplication = await _context.JobApplications.FindAsync(1);
        var updatedUser = await _context.Users.FindAsync("1");

        Assert.Equal("Approved", updatedApplication.Status);
        Assert.Equal(60000, updatedUser.Salary);
        Assert.Single(updatedUser.Teams);
        Assert.Equal(1, updatedUser.Teams.First().TeamId);
    }

    [Fact]
    public async Task GetApproveViewModelAsync_ReturnsViewModel_ForValidApplicationId()
    {
        var existingTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == 1);
        if (existingTeam != null)
        {
            _context.Teams.Remove(existingTeam);
            await _context.SaveChangesAsync();
        }

        var existingJobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == 1);
        if (existingJobPosting != null)
        {
            _context.JobPostings.Remove(existingJobPosting);
            await _context.SaveChangesAsync();
        }

        var existingJobApplication = await _context.JobApplications.FirstOrDefaultAsync(ja => ja.Id == 1);
        if (existingJobApplication != null)
        {
            _context.JobApplications.Remove(existingJobApplication);
            await _context.SaveChangesAsync();
        }

        var team = new Team { Id = 1, Name = "Team A", ManagerId = "1" };
        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            Description = "Develop software applications.",
            RecruiterId = "1",
            PostedDate = DateTime.UtcNow
        };
        var jobApplication = new JobApplication
        {
            Id = 1,
            ApplicantName = "John Doe",
            ApplicantEmail = "john.doe@example.com",
            Status = "Pending",
            ResumeUrl = "http://example.com/resume.pdf",
            JobPostingId = 1,
            JobPosting = jobPosting
        };

        _context.Teams.Add(team);
        _context.JobPostings.Add(jobPosting);
        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync();

        var result = await _service.GetApproveViewModelAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.ApplicationId);
        Assert.Equal("John Doe", result.ApplicantName);
        Assert.Equal("john.doe@example.com", result.ApplicantEmail);
        Assert.Equal("Software Developer", result.JobPostingTitle);
        Assert.NotNull(result.Teams);
        Assert.Single(result.Teams);
        Assert.Equal("1", result.Teams.First().Value);
        Assert.Equal("Team A", result.Teams.First().Text);
    }

    [Fact]
    public async Task GetDenyViewModelAsync_ReturnsViewModel_ForValidApplicationId()
    {
        var existingJobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == 1);
        if (existingJobPosting != null)
        {
            _context.JobPostings.Remove(existingJobPosting);
            await _context.SaveChangesAsync();
        }

        var existingJobApplication = await _context.JobApplications.FirstOrDefaultAsync(ja => ja.Id == 1);
        if (existingJobApplication != null)
        {
            _context.JobApplications.Remove(existingJobApplication);
            await _context.SaveChangesAsync();
        }

        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            Description = "Develop software applications.",
            RecruiterId = "1",
            PostedDate = DateTime.UtcNow
        };
        var jobApplication = new JobApplication
        {
            Id = 1,
            ApplicantName = "John Doe",
            ApplicantEmail = "john.doe@example.com",
            Status = "Pending",
            ResumeUrl = "http://example.com/resume.pdf",
            JobPostingId = 1,
            JobPosting = jobPosting
        };

        _context.JobPostings.Add(jobPosting);
        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync();

        var result = await _service.GetDenyViewModelAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.ApplicationId);
        Assert.Equal("John Doe", result.ApplicantName);
        Assert.Equal("john.doe@example.com", result.ApplicantEmail);
        Assert.Equal("Software Developer", result.JobPostingTitle);
    }

    [Fact]
    public async Task DenyApplicationAsync_SetsApplicationStatusToDenied()
    {
        var existingJobApplication = await _context.JobApplications.FirstOrDefaultAsync(ja => ja.Id == 1);
        if (existingJobApplication != null)
        {
            _context.JobApplications.Remove(existingJobApplication);
            await _context.SaveChangesAsync();
        }

        var jobApplication = new JobApplication
        {
            Id = 1,
            ApplicantName = "John Doe",
            ApplicantEmail = "john.doe@example.com",
            Status = "Pending",
            ResumeUrl = "http://example.com/resume.pdf",
            JobPostingId = 1
        };

        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync();

        var model = new DenyApplicationViewModel
        {
            ApplicationId = 1,
            DenialReason = "Insufficient qualifications."
        };

        await _service.DenyApplicationAsync(model);

        var updatedApplication = await _context.JobApplications.FindAsync(1);

        Assert.NotNull(updatedApplication);
        Assert.Equal("Denied", updatedApplication.Status);
        Assert.Equal("Insufficient qualifications.", updatedApplication.DenialReason);
    }

    [Fact]
    public async Task GetApplyViewModelAsync_ReturnsViewModel_ForValidJobPostingId()
    {
        var existingJobPostings = await _context.JobPostings.ToListAsync();
        _context.JobPostings.RemoveRange(existingJobPostings);
        await _context.SaveChangesAsync();

        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            Description = "Develop applications",
            RecruiterId = "1",
            PostedDate = DateTime.UtcNow
        };
        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync();

        var result = await _service.GetApplyViewModelAsync(1, "john.doe@example.com");

        Assert.NotNull(result);
        Assert.Equal(1, result.JobPostingId);
    }

    [Fact]
    public async Task GetApplyViewModelAsync_ThrowsException_ForInvalidJobPostingId()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetApplyViewModelAsync(99, "john.doe@example.com"));
    }
}