using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_Platform.Controllers
{
    [Authorize]
    public class JobApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Recruiter,Admin")]
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

        [Authorize(Roles = "Recruiter,Admin")]
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

        [Authorize(Roles = "Recruiter,Admin")]
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

            return RedirectToAction("Applicants", new { id = application.JobPostingId });
        }

        [Authorize(Roles = "Recruiter,Admin")]
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

        [Authorize(Roles = "Recruiter,Admin")]
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

        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> ApplicantLog(string? title, string? postedDate, string? recruiter, string? applicantName)
        {
            var jobApplications = _context.JobApplications
                .Include(ja => ja.JobPosting)
                .ThenInclude(jp => jp.Recruiter)
                .Where(ja => ja.Status == "Approved" || ja.Status == "Denied");

            if (!string.IsNullOrEmpty(title))
            {
                title = title.Trim();
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
                    EF.Functions.Like(ja.JobPosting.Recruiter.Email.ToLower(), $"%{recruiter}%") ||
                    EF.Functions.Like((ja.JobPosting.Recruiter.FirstName + " " + ja.JobPosting.Recruiter.LastName).ToLower(), $"%{recruiter}%"));
            }

            if (!string.IsNullOrEmpty(applicantName))
            {
                applicantName = applicantName.Trim().ToLower();
                jobApplications = jobApplications.Where(ja =>
                    EF.Functions.Like(ja.ApplicantName.ToLower(), $"%{applicantName}%"));
            }

            ViewData["TitleFilter"] = title;
            ViewData["PostedDateFilter"] = postedDate;
            ViewData["RecruiterFilter"] = recruiter;
            ViewData["ApplicantNameFilter"] = applicantName;

            var filteredApplications = await jobApplications.ToListAsync();
            return View(filteredApplications);
        }
    }
}