namespace BookingQueue.Common.Models.ViewModels;

public class CompanyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string? IconPath { get; set; }
    public string CompanyLink { get; set; }
    public string CompanyPhone { get; set; }
    public string CompanyMail { get; set; }
    public List<BranchViewModel> Branches { get; set; }

    public CompanyViewModel()
    {
        Branches = new List<BranchViewModel>();
    }
}