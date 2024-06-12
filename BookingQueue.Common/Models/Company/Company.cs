using BookingQueue.Common.Models.ViewModels;

namespace BookingQueue.Common.Models.Company;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string IconPath { get; set; }
    public string CompanyLink { get; set; }
    public string CompanyPhone { get; set; }
    public string CompanyMail { get; set; }
    public List<Branch> Branches { get; set; }
    public User.User User { get; set; }

    public CompanyViewModel ToViewModel()
    {
        return new CompanyViewModel
        {
            Id = Id,
            Name = Name,
            Title = Title,
            IconPath = IconPath,
            CompanyLink = CompanyLink,
            CompanyPhone = CompanyPhone,
            CompanyMail = CompanyMail,
            Branches = Branches.Select(b => new BranchViewModel
            {
                Id = b.Id,
                Name = b.Name,
                Connection = b.Connection,
                CompanyId = b.CompanyId,
                IsProgress = b.IsProgress,
                Address = b.Address
            }).ToList()
        };
    }
}