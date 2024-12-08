using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Employee,Manager")]
public class LeaveRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaveRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Employee,Manager")]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Unable to find your user information.";
            return RedirectToAction("Index", "Home");
        }

        var today = DateTime.UtcNow.Date;

        var leaveRequests = await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == currentUser.Id && lr.EndDate >= today)
            .OrderBy(lr => lr.StartDate)
            .ToListAsync();

        return View(leaveRequests);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Create(LeaveRequestCreateViewModel model)
    {
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

        var teamMember = await _context.TeamMembers
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.UserId == currentUser.Id);

        if (teamMember == null)
        {
            TempData["ErrorMessage"] = "You are not part of any team.";
            return RedirectToAction("Index");
        }

        var hasOverlappingRequests = await _context.LeaveRequests
            .Where(lr =>
                lr.EmployeeId == currentUser.Id &&
                lr.StartDate <= model.EndDate &&
                lr.EndDate >= model.StartDate)
            .AnyAsync();

        if (hasOverlappingRequests)
        {
            TempData["ErrorMessage"] = "You already have a leave request overlapping with the selected dates.";
            return RedirectToAction("Index");
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = currentUser.Id,
            TeamId = teamMember.TeamId,
            StartDate = model.StartDate,
            EndDate = model.EndDate.AddDays(1),
            Status = "Pending",
            ManagerId = teamMember.Team.ManagerId
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your leave request has been submitted successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Review()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Unable to find your user information.";
            return RedirectToAction("Index");
        }

        var leaveRequests = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Where(lr => lr.ManagerId == currentUser.Id && lr.Status == "Pending")
            .ToListAsync();

        return View(leaveRequests);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReject(int id, string action)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);

        if (leaveRequest == null)
        {
            TempData["ErrorMessage"] = "Leave request not found.";
            return RedirectToAction("Review");
        }

        if (leaveRequest.ManagerId != (await _userManager.GetUserAsync(User)).Id)
        {
            TempData["ErrorMessage"] = "You are not authorized to process this leave request.";
            return RedirectToAction("Review");
        }

        if (action == "Approve")
        {
            leaveRequest.Status = "Approved";
        }
        else if (action == "Reject")
        {
            leaveRequest.Status = "Rejected";
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Leave request has been {leaveRequest.Status.ToLower()} successfully.";
        return RedirectToAction("Review");
    }
}