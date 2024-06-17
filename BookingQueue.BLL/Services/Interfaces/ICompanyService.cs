using BookingQueue.Common.Models.Company;

namespace BookingQueue.BLL.Services.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<Company>> GetCompaniesAsync();
    Task<IEnumerable<Branch>> GetBranchesByCompanyIdAsync(int companyId);
    Task<Company?> GetCompanyByIdAsync(int id);
    Task CreateOrUpdateCompanyAsync(Company company);
    Task CreateOrUpdateBranchesAsync(int companyId, List<Branch> branches);
    Task<(IEnumerable<Company> Companies, int TotalCount)> GetCompaniesAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Company>> GetAllCompaniesWithBranchesAsync();
    Task DeleteCompany(int companyId);
}