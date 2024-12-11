using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin,Manager,HR,Recruiter")]
public class TeamsController : Controller
{
    private readonly TeamsService _teamsService;

    public TeamsController(TeamsService teamsService)
    {
        _teamsService = teamsService;
    }

    public async Task<IActionResult> Index()
    {
        var teams = await _teamsService.GetAllTeamsAsync();
        return View(teams);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var team = await _teamsService.GetTeamByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        await _teamsService.PopulateManagersDropdown(ViewData, team.ManagerId);
        return View(team);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Team team)
    {
        if (id != team.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _teamsService.UpdateTeamAsync(team);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        await _teamsService.PopulateManagersDropdown(ViewData, team.ManagerId);
        return View(team);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var team = await _teamsService.GetTeamDetailsAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await _teamsService.PopulateManagersDropdown(ViewData);
        return View();
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Team team)
    {
        if (ModelState.IsValid)
        {
            var result = await _teamsService.CreateTeamAsync(team);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        await _teamsService.PopulateManagersDropdown(ViewData);
        return View(team);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _teamsService.GetTeamByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _teamsService.DeleteTeamAsync(id);
        if (result)
        {
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Delete), new { id });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int teamId, string userId)
    {
        var result = await _teamsService.RemoveTeamMemberAsync(teamId, userId);
        if (!result)
        {
            TempData["ErrorMessage"] = "Failed to remove team member.";
        }

        return RedirectToAction("Details", new { id = teamId });
    }
}