using System.ComponentModel.DataAnnotations;

public class JobApplicationLogViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ApplicantName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string ApplicantEmail { get; set; } = null!;

    [Required]
    [Url]
    public string ResumeUrl { get; set; } = null!;

    [Required]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string? DenialReason { get; set; }

    public int JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
}