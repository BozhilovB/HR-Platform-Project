using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Employee,Manager,HR,Recruiter,Admin")]
public class LeaveRequestsController : Controller
{
    private readonly LeaveRequestsService _leaveRequestsService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaveRequestsController(LeaveRequestsService leaveRequestsService, UserManager<ApplicationUser> userManager)
    {
        _leaveRequestsService = leaveRequestsService;
        _userManager = userManager;
    }

    [Authorize(Roles = "Employee,Manager,HR,Recruiter,Admin")]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Unable to find your user information.";
            return RedirectToAction("Index", "Home");
        }

        var leaveRequests = await _leaveRequestsService.GetUserLeaveRequestsAsync(currentUser.Id, DateTime.UtcNow.Date);
        return View(leaveRequests);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequestCreateViewModel model)
    {
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Unable to find your user information.";
            return RedirectToAction("Index");
        }

        var teamMember = await _leaveRequestsService.GetTeamMemberAsync(currentUser.Id);
        if (teamMember == null)
        {
            TempData["ErrorMessage"] = "You are not part of any team.";
            return RedirectToAction("Index");
        }

        if (await _leaveRequestsService.HasOverlappingLeaveRequestAsync(currentUser.Id, model.StartDate, model.EndDate))
        {
            TempData["ErrorMessage"] = "You already have a leave request overlapping with the selected dates.";
            return RedirectToAction("Index");
        }

        await _leaveRequestsService.SubmitLeaveRequestAsync(currentUser.Id, teamMember.TeamId, model.StartDate, model.EndDate, teamMember.Team.ManagerId);

        TempData["SuccessMessage"] = "Your leave request has been submitted successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Review()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Unable to find your user information.";
            return RedirectToAction("Index");
        }

        var isAdmin = User.IsInRole("Admin");
        var leaveRequests = await _leaveRequestsService.GetLeaveRequestsForReviewAsync(currentUser.Id, isAdmin);
        return View(leaveRequests);
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReject(int id, string action)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var leaveRequest = await _leaveRequestsService.GetLeaveRequestByIdAsync(id);

            if (leaveRequest.ManagerId != currentUser.Id && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "You are not authorized to process this leave request.";
                return RedirectToAction("Review");
            }

            var status = action == "Approve" ? "Approved" : "Rejected";
            await _leaveRequestsService.UpdateLeaveRequestStatusAsync(id, status);

            TempData["SuccessMessage"] = $"Leave request has been {status.ToLower()} successfully.";
            return RedirectToAction("Review");
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Leave request not found.";
            return RedirectToAction("Review");
        }
    }
}