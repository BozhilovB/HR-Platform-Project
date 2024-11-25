using System.ComponentModel.DataAnnotations;
public class TeamMember
{
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
}