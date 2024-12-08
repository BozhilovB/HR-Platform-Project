using System.ComponentModel.DataAnnotations;

public class LeaveRequestCreateViewModel
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
}