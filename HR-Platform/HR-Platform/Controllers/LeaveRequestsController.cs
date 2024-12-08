using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Employee,Manager")]
public class LeaveRequestsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}