using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class JobPostingsController : Controller
{
    private readonly JobPostingsService _jobPostingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JobPostingsController(JobPostingsService jobPostingsService, IHttpContextAccessor httpContextAccessor)
    {
        _jobPostingsService = jobPostingsService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IActionResult> Index()
    {
        var jobPostings = await _jobPostingsService.GetJobPostingsAsync();
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

        var recruiterId = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _jobPostingsService.CreateJobPostingAsync(model, recruiterId);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var jobPosting = await _jobPostingsService.GetJobPostingByIdAsync(id);
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

        try
        {
            await _jobPostingsService.UpdateJobPostingAsync(id, model);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _jobPostingsService.DeleteJobPostingAsync(id);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Applicants(int id)
    {
        var jobApplications = await _jobPostingsService.GetJobApplicantsAsync(id);

        if (!jobApplications.Any())
        {
            return View(new List<JobApplication>());
        }

        return View(jobApplications);
    }
}