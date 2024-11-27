using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Manager,HR,Recruiter")]
public class TeamsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeamsController(ApplicationDbContext dbContext)
    {
        this._context = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var teams = await _context.Teams
            .Include(t => t.Manager)
            .ToListAsync();

        return View(teams);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        await PopulateManagersDropdown(team.ManagerId);
        return View(team);
    }


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
            try
            {
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existingTeam == null)
                {
                    return NotFound();
                }

                // Update fields explicitly
                existingTeam.Name = team.Name;
                existingTeam.ManagerId = team.ManagerId;

                // Save changes
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Re-populate managers dropdown in case of errors
        await PopulateManagersDropdown(team.ManagerId);
        return View(team);
    }












    private async Task PopulateManagersDropdown(string? selectedManagerId = null)
    {
        var managerRoleId = await _context.Roles
            .Where(r => r.Name == "Manager")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var managers = await _context.UserRoles
            .Where(ur => ur.RoleId == managerRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var managerList = _context.Users.Where(u => managers.Contains(u.Id)).ToList();

        ViewData["Managers"] = new SelectList(managerList, "Id", "UserName", selectedManagerId);
    }



}