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
}