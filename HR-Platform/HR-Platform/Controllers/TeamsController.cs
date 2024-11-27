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

                existingTeam.Name = team.Name;
                existingTeam.ManagerId = team.ManagerId;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        await PopulateManagersDropdown(team.ManagerId);
        return View(team);
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.TeamMembers)
            .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }




    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateManagersDropdown();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Team team)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Save the new team
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        await PopulateManagersDropdown();
        return View(team);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
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