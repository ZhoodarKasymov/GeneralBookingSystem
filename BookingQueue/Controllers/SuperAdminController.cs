using System.Security.Cryptography;
using System.Text;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Models.Company;
using BookingQueue.Common.Models.ViewModels;
using BookingQueue.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingQueue.Controllers;

public class SuperAdminController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly FileUploadService _fileUploadService;
    private const int PageSize = 15;

    public SuperAdminController(ICompanyService companyService, FileUploadService fileUploadService)
    {
        _companyService = companyService;
        _fileUploadService = fileUploadService;
    }

    [HttpGet("/admin/manage")]
    public async Task<IActionResult> SuperAdminPage(int pageNumber = 1)
    {
        var (companies, totalCount) = await _companyService.GetCompaniesAsync(pageNumber, PageSize);

        var viewModel = new CompanyIndexViewModel
        {
            Companies = companies,
            Pagination = new PaginationViewModel
            {
                CurrentPage = pageNumber,
                PageSize = PageSize,
                TotalItems = totalCount
            }
        };

        return View(viewModel);
    }
    
    [HttpGet("/admin/create-company")]
    public async Task<IActionResult> CreateCompany(int? companyId)
    {
        if (companyId is null or 0) return View(new CompanyViewModel());
        
        var company = await _companyService.GetCompanyByIdAsync(companyId.Value);
        
        if (company is null) return View(new CompanyViewModel());
        
        var companyViewModel = company.ToViewModel();
        return View("CreateCompany", companyViewModel);
    }

    [HttpPost("/admin/create-company")]
    public async Task<IActionResult> CreateCompany(CompanyViewModel model, IFormFile? iconFile)
    {
        if (ModelState.IsValid)
        {
            var company = new Company
            {
                Id = model.Id,
                Name = model.Name,
                CompanyPhone = model.CompanyPhone,
                CompanyMail = model.CompanyMail,
                CompanyLink = model.CompanyLink,
                Title = model.Title,
                IconPath = model.IconPath ?? ""
            };
            
            if (iconFile is not null)
            {
                var iconPath = _fileUploadService.UploadFile(iconFile);
                company.IconPath = iconPath;
            }

            await _companyService.CreateOrUpdateCompanyAsync(company);
            
            // Add branches to the company
            company.Branches = model.Branches.Select(branch => new Branch
            {
                Name = branch.Name,
                Connection = branch.Connection,
                IsProgress = branch.IsProgress,
                CompanyId = company.Id,
                Id = branch.Id,
                Address = branch.Address
            }).ToList();

            await _companyService.CreateOrUpdateBranchesAsync(company.Id, company.Branches);
            
            return RedirectToAction("SuperAdminPage");
        }

        return View(model);
    }

    #region Private methods

    private string HashPassword(string password)
    {
        var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    #endregion
}