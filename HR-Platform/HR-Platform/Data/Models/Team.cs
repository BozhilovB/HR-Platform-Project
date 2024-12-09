using System.ComponentModel.DataAnnotations;

public class Team
{
    public int Id { get; set; }

    [Required]
    [MinLength(Validations.TeamNameMinLength)]
    [MaxLength(Validations.TeamNameMaxLength)]
    public string Name { get; set; } = null!;

    [Required]
    public string ManagerId { get; set; } = null!;

    public ApplicationUser? Manager { get; set; }

    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}