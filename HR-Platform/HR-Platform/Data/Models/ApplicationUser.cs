using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MinLength(Validations.UserFirstNameMinLength)]
    [MaxLength(Validations.UserFirstNameMaxLength)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(Validations.UserLastNameMinLength)]
    [MaxLength(Validations.UserLastNameMaxLength)]
    public string LastName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<TeamMember> Teams { get; set; } = new List<TeamMember>();
}