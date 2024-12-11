using HR_Platform.Data;
using HR_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class JobApplicationsService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobApplicationsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<JobApplication>> GetApplicantsAsync(int jobPostingId)
    {
        return await _context.JobApplications
            .Where(ja => ja.JobPostingId == jobPostingId && ja.Status != "Denied" && ja.Status != "Approved")
            .Include(ja => ja.JobPosting)
            .ToListAsync();
    }

    public async Task<ApproveApplicationViewModel> GetApproveViewModelAsync(int applicationId)
    {
        var application = await _context.JobApplications
            .Include(ja => ja.JobPosting)
            .FirstOrDefaultAsync(ja => ja.Id == applicationId);

        if (application == null)
            return null;

        var teams = _context.Teams.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.Name
        }).ToList();

        return new ApproveApplicationViewModel
        {
            ApplicationId = applicationId,
            ApplicantName = application.ApplicantName,
            ApplicantEmail = application.ApplicantEmail,
            Teams = teams,
            JobPostingTitle = application.JobPosting.Title
        };
    }

    public async Task ApproveApplicationAsync(ApproveApplicationViewModel model)
    {
        var applicant = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.ApplicantEmail);
        if (applicant == null)
            throw new KeyNotFoundException("Applicant not found.");

        if (await _userManager.IsInRoleAsync(applicant, "User"))
        {
            await _userManager.RemoveFromRoleAsync(applicant, "User");
        }

        if (!await _userManager.IsInRoleAsync(applicant, "Employee"))
        {
            await _userManager.AddToRoleAsync(applicant, "Employee");
        }

        applicant.Salary = model.Salary;
        applicant.Teams.Add(new TeamMember
        {
            TeamId = model.SelectedTeamId,
            UserId = applicant.Id,
            JoinedAt = DateTime.UtcNow
        });

        var application = await _context.JobApplications.FindAsync(model.ApplicationId);
        if (application != null)
        {
            application.Status = "Approved";
        }

        await _context.SaveChangesAsync();
    }

    public async Task<DenyApplicationViewModel> GetDenyViewModelAsync(int applicationId)
    {
        var application = await _context.JobApplications
            .Include(ja => ja.JobPosting)
            .FirstOrDefaultAsync(ja => ja.Id == applicationId);

        if (application == null)
            return null;

        return new DenyApplicationViewModel
        {
            ApplicationId = applicationId,
            ApplicantName = application.ApplicantName,
            ApplicantEmail = application.ApplicantEmail,
            JobPostingTitle = application.JobPosting.Title
        };
    }

    public async Task DenyApplicationAsync(DenyApplicationViewModel model)
    {
        var application = await _context.JobApplications.FindAsync(model.ApplicationId);
        if (application == null)
            throw new KeyNotFoundException("Application not found.");

        application.Status = "Denied";
        application.DenialReason = model.DenialReason;

        await _context.SaveChangesAsync();
    }

    public async Task<List<JobApplication>> GetFilteredApplicationsAsync(string? title, string? postedDate, string? recruiter, string? applicantName)
    {
        var jobApplications = _context.JobApplications
            .Include(ja => ja.JobPosting)
            .ThenInclude(jp => jp.Recruiter)
            .Where(ja => ja.Status == "Approved" || ja.Status == "Denied");

        if (!string.IsNullOrEmpty(title))
        {
            jobApplications = jobApplications.Where(ja => EF.Functions.Like(ja.JobPosting.Title, $"%{title}%"));
        }

        if (!string.IsNullOrEmpty(postedDate) && DateTime.TryParse(postedDate, out var parsedDate))
        {
            jobApplications = jobApplications.Where(ja => ja.JobPosting.PostedDate.Date == parsedDate.Date);
        }

        if (!string.IsNullOrEmpty(recruiter))
        {
            recruiter = recruiter.Trim().ToLower();
            jobApplications = jobApplications.Where(ja =>
                EF.Functions.Like(ja.JobPosting.Recruiter.FirstName.ToLower(), $"%{recruiter}%") ||
                EF.Functions.Like(ja.JobPosting.Recruiter.LastName.ToLower(), $"%{recruiter}%") ||
                EF.Functions.Like(ja.JobPosting.Recruiter.Email.ToLower(), $"%{recruiter}%"));
        }

        if (!string.IsNullOrEmpty(applicantName))
        {
            applicantName = applicantName.Trim().ToLower();
            jobApplications = jobApplications.Where(ja => EF.Functions.Like(ja.ApplicantName.ToLower(), $"%{applicantName}%"));
        }

        return await jobApplications.ToListAsync();
    }

    public async Task<ApplyJobViewModel> GetApplyViewModelAsync(int jobPostingId, string userEmail)
    {
        var jobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == jobPostingId);

        if (jobPosting == null)
            throw new KeyNotFoundException("Job posting not found.");

        return new ApplyJobViewModel
        {
            JobPostingId = jobPostingId
        };
    }

    public async Task ApplyForJobAsync(ApplyJobViewModel model, string userEmail)
    {
        var currentUser = await _userManager.FindByEmailAsync(userEmail);

        if (currentUser == null)
            throw new UnauthorizedAccessException("Unable to find your user information.");

        var hasPendingApplication = await _context.JobApplications
            .AnyAsync(ja => ja.ApplicantEmail.ToLower() == userEmail.ToLower() && ja.Status == "Pending");

        if (hasPendingApplication)
            throw new InvalidOperationException("You already have a pending application.");

        var jobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == model.JobPostingId);

        if (jobPosting == null)
            throw new KeyNotFoundException("Job posting not found.");

        var jobApplication = new JobApplication
        {
            ApplicantName = $"{currentUser.FirstName} {currentUser.LastName}",
            ApplicantEmail = userEmail,
            ResumeUrl = model.ResumeUrl,
            Status = "Pending",
            JobPostingId = jobPosting.Id
        };

        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync();
    }
}