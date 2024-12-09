using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Users(string searchTerm, string team, string role)
    {
        var usersQuery = _context.Users
            .Include(u => u.Teams)
            .ThenInclude(tm => tm.Team)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            var searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            usersQuery = usersQuery.Where(u =>
                searchTerms.All(st =>
                    u.FirstName.ToLower().Contains(st) ||
                    u.LastName.ToLower().Contains(st) ||
                    u.Email.ToLower().Contains(st)));
        }

        if (!string.IsNullOrEmpty(team))
        {
            usersQuery = usersQuery.Where(u =>
                u.Teams.Any(tm => tm.Team.Name.ToLower().Contains(team.ToLower())));
        }

        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = usersInRole.Select(u => u.Id).ToList();
            usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
        }

        var users = await usersQuery.ToListAsync();
        ViewData["SearchTerm"] = searchTerm;
        ViewData["TeamFilter"] = team;
        ViewData["RoleFilter"] = role;

        return View(users);
    }
}