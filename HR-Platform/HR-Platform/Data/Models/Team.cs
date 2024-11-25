using System.ComponentModel.DataAnnotations;

public class Team
{
    public int Id { get; set; }

    [Required]
    [MinLength(Validations.TeamNameMinLength)]
    [MaxLength(Validations.TeamNameMaxLength)]
    public string Name { get; set; } = null!;

    public string ManagerId { get; set; } = null!;

    public ApplicationUser Manager { get; set; } = null!;

    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}