using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Models.Company;
using Microsoft.AspNetCore.Mvc;

namespace BookingQueue.Controllers.API;

[ApiController]
public class ApiController : Controller
{
    private readonly ICompanyService _companyService;

    public ApiController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>
    /// Для получения списка компаний и филиалов
    /// </summary>
    /// <returns>Список компаний и филиалов</returns>
    [HttpGet("/api/companies")]
    public async Task<IEnumerable<Company>> Companies()
    {
        return await _companyService.GetAllCompaniesWithBranchesAsync();
    }
    
    
}