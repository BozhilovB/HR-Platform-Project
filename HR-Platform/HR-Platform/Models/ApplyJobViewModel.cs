using System.ComponentModel.DataAnnotations;

public class ApplyJobViewModel
{
    public int JobPostingId { get; set; }

    [Required]
    [Url]
    public string ResumeUrl { get; set; }
}