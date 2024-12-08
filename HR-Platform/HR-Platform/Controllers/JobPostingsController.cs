using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class JobPostingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public JobPostingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var jobPostings = await _context.JobPostings
            .Include(jp => jp.Recruiter)
            .ToListAsync();

        return View(jobPostings);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPostingCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var recruiterId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var jobPosting = new JobPosting
        {
            Title = model.Title,
            Description = model.Description,
            PostedDate = DateTime.UtcNow,
            RecruiterId = recruiterId
        };

        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var jobPosting = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == id);
        if (jobPosting == null)
        {
            return NotFound();
        }

        var model = new JobPostingEditViewModel
        {
            Id = jobPosting.Id,
            Title = jobPosting.Title,
            Description = jobPosting.Description
        };

        return View(model);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobPostingEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingJob = await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == id);
        if (existingJob == null)
        {
            return NotFound();
        }

        existingJob.Title = model.Title;
        existingJob.Description = model.Description;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var jobPosting = await _context.JobPostings.FindAsync(id);
        if (jobPosting == null)
        {
            return NotFound();
        }

        _context.JobPostings.Remove(jobPosting);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Applicants(int id)
    {
        var jobApplications = await _context.JobApplications
            .Where(ja => ja.JobPostingId == id)
            .ToListAsync();

        if (!jobApplications.Any())
        {
            return View(new List<JobApplication>());
        }

        return View(jobApplications);
    }
}
