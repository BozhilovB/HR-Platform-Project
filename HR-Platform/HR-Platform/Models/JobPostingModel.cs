using System.ComponentModel.DataAnnotations;

public class JobPostings
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters.")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 1000 characters.")]
    public string Description { get; set; } = null!;

    public DateTime PostedDate { get; set; }

    [Required]
    public string RecruiterId { get; set; } = null!;

    public ApplicationUser Recruiter { get; set; } = null!;
}