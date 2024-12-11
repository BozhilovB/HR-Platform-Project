using Microsoft.EntityFrameworkCore;

public class JobPostingsService
{
    private readonly ApplicationDbContext _context;

    public JobPostingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobPosting>> GetJobPostingsAsync()
    {
        return await _context.JobPostings
            .Include(jp => jp.Recruiter)
            .ToListAsync();
    }

    public async Task CreateJobPostingAsync(JobPostingCreateViewModel model, string recruiterId)
    {
        var jobPosting = new JobPosting
        {
            Title = model.Title,
            Description = model.Description,
            PostedDate = DateTime.UtcNow,
            RecruiterId = recruiterId
        };

        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync();
    }

    public async Task<JobPosting> GetJobPostingByIdAsync(int id)
    {
        return await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == id);
    }

    public async Task UpdateJobPostingAsync(int id, JobPostingEditViewModel model)
    {
        var existingJob = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == id);
        if (existingJob == null)
        {
            throw new KeyNotFoundException("Job posting not found.");
        }

        existingJob.Title = model.Title;
        existingJob.Description = model.Description;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteJobPostingAsync(int id)
    {
        var jobPosting = await _context.JobPostings.FindAsync(id);
        if (jobPosting == null)
        {
            throw new KeyNotFoundException("Job posting not found.");
        }

        _context.JobPostings.Remove(jobPosting);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JobApplication>> GetJobApplicantsAsync(int jobId)
    {
        return await _context.JobApplications
            .Where(ja => ja.JobPostingId == jobId)
            .ToListAsync();
    }
}