using Microsoft.AspNetCore.Mvc.Rendering;

public class ApproveApplicationViewModel
{
    public int ApplicationId { get; set; }

    public string ApplicantName { get; set; } = null!;

    public string ApplicantEmail { get; set; } = null!;

    public decimal Salary { get; set; }

    public IEnumerable<SelectListItem> Teams { get; set; } = new List<SelectListItem>();

    public int SelectedTeamId { get; set; }

    public string JobPostingTitle { get; set; } = null!;
}
