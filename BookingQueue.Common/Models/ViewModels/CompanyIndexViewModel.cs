namespace BookingQueue.Common.Models.ViewModels;

public class CompanyIndexViewModel
{
    public IEnumerable<Company.Company> Companies { get; set; }
    public PaginationViewModel Pagination { get; set; }
}