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

    [Authorize(Roles = "Recruiter")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Recruiter")]
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


    [Authorize(Roles = "Recruiter")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var jobPosting = await _context.JobPostings.FindAsync(id);
        if (jobPosting == null)
        {
            return NotFound();
        }

        return View(jobPosting);
    }

    [Authorize(Roles = "Recruiter")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobPosting jobPosting)
    {
        if (id != jobPosting.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(jobPosting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(jobPosting);
    }

    [Authorize(Roles = "Recruiter")]
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

}
