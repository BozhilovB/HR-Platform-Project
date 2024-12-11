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
        private readonly JobApplicationsService _jobApplicationsService;

        public JobApplicationsController(JobApplicationsService jobApplicationsService)
        {
            _jobApplicationsService = jobApplicationsService;
        }

        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var userEmail = User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            try
            {
                var hasPendingApplication = await _jobApplicationsService.HasPendingApplicationAsync(userEmail);

                if (hasPendingApplication)
                {
                    TempData["ErrorMessage"] = "You already have a pending application. Please wait for its outcome before applying for another job.";
                    return RedirectToAction("Index", "JobPostings");
                }

                var model = await _jobApplicationsService.GetApplyViewModelAsync(id, userEmail);
                return View(model);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "JobPostings");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyJobViewModel model)
        {
            var userEmail = User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _jobApplicationsService.ApplyForJobAsync(model, userEmail);
                TempData["SuccessMessage"] = "You have successfully applied for the job!";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "JobPostings");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }

            return RedirectToAction("Index", "JobPostings");
        }

        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Applicants(int id)
        {
            var jobApplications = await _jobApplicationsService.GetApplicantsAsync(id);
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
            var viewModel = await _jobApplicationsService.GetApproveViewModelAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApproveApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _jobApplicationsService.ApproveApplicationAsync(model);
                TempData["SuccessMessage"] = "The application has been approved successfully!";
                return RedirectToAction("Applicants", new { id = model.ApplicationId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        [Authorize(Roles = "Recruiter,Admin")]
        [HttpGet]
        public async Task<IActionResult> Deny(int id)
        {
            var viewModel = await _jobApplicationsService.GetDenyViewModelAsync(id);
            if (viewModel == null)
                return NotFound();

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

            try
            {
                await _jobApplicationsService.DenyApplicationAsync(model);
                TempData["SuccessMessage"] = "The application has been denied successfully!";
                return RedirectToAction("Applicants", new { id = model.ApplicationId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> ApplicantLog(string? title, string? postedDate, string? recruiter, string? applicantName)
        {
            var applications = await _jobApplicationsService.GetFilteredApplicationsAsync(title, postedDate, recruiter, applicantName);
            return View(applications);
        }
    }
}