using System.Collections.Generic;

public class ProfileViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public Team? ManagedTeam { get; set; }
    public List<TeamMember> MemberTeams { get; set; } = new();
    public bool IsOwnProfile { get; set; }
}