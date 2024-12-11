using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? team, string? role)
    {
        var users = await _adminService.GetUsersAsync(searchTerm, team, role);
        ViewData["SearchTerm"] = searchTerm;
        ViewData["TeamFilter"] = team;
        ViewData["RoleFilter"] = role;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _adminService.GetUserByIdAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        var allRoles = await _adminService.GetAllRolesAsync();
        var allTeams = await _adminService.GetAllTeamsAsync(user.Id);
        var userRoles = await _adminService.GetUserRolesAsync(user);

        var viewModel = new UserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            SelectedRoles = userRoles,
            SelectedTeamIds = user.Teams.Select(t => t.TeamId.ToString()).ToList(),
            Roles = allRoles,
            Teams = allTeams
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid data.";
            return RedirectToAction("Index");
        }

        var user = await _adminService.GetUserByIdAsync(model.Id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        await _adminService.UpdateUserAsync(user, model);

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        try
        {
            await _adminService.DeleteUserAsync(user);
            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

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