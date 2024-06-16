namespace BookingQueue.Common.Models.ViewModels;

public class BranchViewModel
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; }
    public string Connection { get; set; }
    public bool IsProgress { get; set; }
    public string Address { get; set; }
    public bool ToDelete { get; set; }
}