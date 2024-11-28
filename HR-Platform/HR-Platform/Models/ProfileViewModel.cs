using System.Collections.Generic;

public class ProfileViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<Team>? ManagedTeams { get; set; } = new();
    public List<TeamMember> MemberTeams { get; set; } = new();
    public bool IsOwnProfile { get; set; }
    public decimal? Salary { get; set; }
}