using System.ComponentModel.DataAnnotations;

public class JobApplication
{
	public int Id { get; set; }

	[Required]
	[MinLength(Validations.ApplicantNameMinLength)]
	[MaxLength(Validations.ApplicantNameMaxLength)]
	public string ApplicantName { get; set; } = null!;

	[Required]
	[MinLength(Validations.ApplicantEmailMinLength)]
	[MaxLength(Validations.ApplicantEmailMaxLength)]
	[EmailAddress]
	public string ApplicantEmail { get; set; } = null!;

	[Required]
	public string ResumeUrl { get; set; } = null!;

	[Required]
	public string Status { get; set; } = null!;

	public int JobPostingId { get; set; }
	public JobPosting JobPosting { get; set; } = null!;

	public string? DenialReason { get; set; }
}