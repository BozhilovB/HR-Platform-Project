using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LeaveRequest
{
    public int Id { get; set; }
    [Required]
    public string EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public ApplicationUser Employee { get; set; }
    [Required]
    public int TeamId { get; set; }
    [ForeignKey("TeamId")]
    public Team Team { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public string Status { get; set; } = "Pending";
    public string? ManagerId { get; set; }
    [ForeignKey("ManagerId")]
    public ApplicationUser? Manager { get; set; }
}