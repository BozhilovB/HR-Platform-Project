using Microsoft.AspNetCore.Mvc.Rendering;

public class UserEditViewModel
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<string> SelectedRoles { get; set; } = new();
    public List<string> SelectedTeamIds { get; set; } = new();
    public List<SelectListItem> Roles { get; set; } = new();
    public List<SelectListItem> Teams { get; set; } = new();
}