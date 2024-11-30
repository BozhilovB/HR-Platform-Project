using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_Platform.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class JobApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

		[Authorize(Roles = "Recruiter")]
		public async Task<IActionResult> Applicants(int id)
		{
			var jobApplications = await _context.JobApplications
				.Where(ja => ja.JobPostingId == id && ja.Status != "Denied" && ja.Status != "Approved")
				.Include(ja => ja.JobPosting)
				.ToListAsync();

			if (!jobApplications.Any())
			{
				return View(new List<JobApplication>());
			}

			ViewBag.JobPostingId = id;
			ViewBag.JobPostingTitle = jobApplications.FirstOrDefault()?.JobPosting.Title;

			return View(jobApplications);
		}

		[HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var application = await _context.JobApplications
                .Include(ja => ja.JobPosting)
                .FirstOrDefaultAsync(ja => ja.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            var teams = _context.Teams.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            var viewModel = new ApproveApplicationViewModel
            {
                ApplicationId = id,
                ApplicantName = application.ApplicantName,
                ApplicantEmail = application.ApplicantEmail,
                Teams = teams,
                JobPostingTitle = application.JobPosting.Title
            };

            return View("~/Views/JobApplications/Approve.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApproveApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var teams = _context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

                model.Teams = teams;
                return View(model);
            }

            var applicant = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.ApplicantEmail);
            if (applicant == null)
            {
                return NotFound();
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

            return RedirectToAction("Applicants", new { id = application.JobPostingId });
        }
		[HttpGet]
		public async Task<IActionResult> Deny(int id)
		{
			var application = await _context.JobApplications
				.Include(ja => ja.JobPosting)
				.FirstOrDefaultAsync(ja => ja.Id == id);

			if (application == null)
			{
				return NotFound();
			}

			var viewModel = new DenyApplicationViewModel
			{
				ApplicationId = id,
				ApplicantName = application.ApplicantName,
				ApplicantEmail = application.ApplicantEmail,
				JobPostingTitle = application.JobPosting.Title
			};

			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deny(DenyApplicationViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var application = await _context.JobApplications.FindAsync(model.ApplicationId);
			if (application == null)
			{
				return NotFound();
			}

			application.Status = "Denied";
			application.DenialReason = model.DenialReason;

			await _context.SaveChangesAsync();

			return RedirectToAction("Applicants", new { id = application.JobPostingId });
		}

	}
}
