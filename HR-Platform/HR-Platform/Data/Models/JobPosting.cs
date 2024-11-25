using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;

public class JobPosting
{
    public int Id { get; set; }

    [Required]
    [MinLength(Validations.JobTitleMinLength)]
    [MaxLength(Validations.JobTitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(Validations.JobDescriptionMinLength)]
    [MaxLength(Validations.JobDescriptionMaxLength)]
    public string Description { get; set; } = null!;

    public DateTime PostedDate { get; set; }

    public string RecruiterId { get; set; } = null!;

    public ApplicationUser Recruiter { get; set; } = null!;

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}