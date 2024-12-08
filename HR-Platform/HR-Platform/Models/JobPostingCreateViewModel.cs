using System.ComponentModel.DataAnnotations;

public class JobPostingCreateViewModel
{
    [Required]
    [MinLength(Validations.JobTitleMinLength)]
    [MaxLength(Validations.JobTitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(Validations.JobDescriptionMinLength)]
    [MaxLength(Validations.JobDescriptionMaxLength)]
    public string Description { get; set; } = null!;
}