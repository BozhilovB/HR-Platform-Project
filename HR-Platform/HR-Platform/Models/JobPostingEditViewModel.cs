using System.ComponentModel.DataAnnotations;

public class JobPostingEditViewModel
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
}