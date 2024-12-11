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
            try
            {
                await _jobApplicationsService.DenyApplicationAsync(model);
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