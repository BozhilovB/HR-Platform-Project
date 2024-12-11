using Microsoft.EntityFrameworkCore;

public class JobPostingsServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly JobPostingsService _service;

    public JobPostingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "JobPostingsTestDb")
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new JobPostingsService(_context);
        ClearDatabase();
    }

    public void ClearDatabase()
    {
        _context.JobPostings.RemoveRange(_context.JobPostings);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateJobPostingAsync_CreatesNewJobPosting()
    {
        var model = new JobPostingCreateViewModel
        {
            Title = "Backend Developer",
            Description = "Develop APIs and backend systems."
        };
        var recruiterId = "1";

        await _service.CreateJobPostingAsync(model, recruiterId);

        var jobPostings = await _context.JobPostings.ToListAsync();
        Assert.Single(jobPostings);
        Assert.Equal("Backend Developer", jobPostings[0].Title);
        Assert.Equal("Develop APIs and backend systems.", jobPostings[0].Description);
        Assert.Equal(recruiterId, jobPostings[0].RecruiterId);
        Assert.True(jobPostings[0].PostedDate <= DateTime.UtcNow && jobPostings[0].PostedDate > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task GetJobPostingByIdAsync_ReturnsJobPosting_ForValidId()
    {
        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Frontend Developer",
            Description = "Develop user interfaces.",
            PostedDate = DateTime.UtcNow,
            RecruiterId = "1"
        };

        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync();

        var result = await _service.GetJobPostingByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Frontend Developer", result.Title);
        Assert.Equal("Develop user interfaces.", result.Description);
        Assert.Equal("1", result.RecruiterId);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetJobPostingByIdAsync_ReturnsNull_ForInvalidId()
    {
        var result = await _service.GetJobPostingByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateJobPostingAsync_UpdatesJobPosting_ForValidId()
    {
        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            Description = "Old Description",
            PostedDate = DateTime.UtcNow,
            RecruiterId = "1"
        };

        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync();

        var updatedModel = new JobPostingEditViewModel
        {
            Title = "Updated Software Developer",
            Description = "Updated Description"
        };

        await _service.UpdateJobPostingAsync(1, updatedModel);

        var updatedJobPosting = await _context.JobPostings.FindAsync(1);
        Assert.NotNull(updatedJobPosting);
        Assert.Equal("Updated Software Developer", updatedJobPosting.Title);
        Assert.Equal("Updated Description", updatedJobPosting.Description);
    }

    [Fact]
    public async Task UpdateJobPostingAsync_ThrowsException_ForInvalidId()
    {
        var updatedModel = new JobPostingEditViewModel
        {
            Title = "Updated Software Developer",
            Description = "Updated Description"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateJobPostingAsync(99, updatedModel));
    }

    [Fact]
    public async Task DeleteJobPostingAsync_DeletesJobPosting_ForValidId()
    {
        var jobPosting = new JobPosting
        {
            Id = 1,
            Title = "Software Developer",
            Description = "Develop software applications.",
            PostedDate = DateTime.UtcNow,
            RecruiterId = "1"
        };

        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync();
        await _service.DeleteJobPostingAsync(1);

        var deletedJobPosting = await _context.JobPostings.FindAsync(1);
        Assert.Null(deletedJobPosting);
    }

    [Fact]
    public async Task DeleteJobPostingAsync_ThrowsException_ForInvalidId()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteJobPostingAsync(99));
    }
}