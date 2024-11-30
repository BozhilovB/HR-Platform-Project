using System.ComponentModel.DataAnnotations;

public class DenyApplicationViewModel
{
	public int ApplicationId { get; set; }
	public string ApplicantName { get; set; } = null!;
	public string ApplicantEmail { get; set; } = null!;
	public string JobPostingTitle { get; set; } = null!;

	[Required(ErrorMessage = "Please provide a reason for denial.")]
	public string DenialReason { get; set; } = null!;
}