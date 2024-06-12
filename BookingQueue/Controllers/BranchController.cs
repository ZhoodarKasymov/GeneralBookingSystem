using System.Globalization;
using BookingQueue.BLL.Resources;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BookingQueue.Controllers;

public class BranchController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly LocService _localization;
    private readonly ICompanyService _companyService;

    public BranchController
    (
        IWebHostEnvironment hostEnvironment, 
        LocService localization,
        ICompanyService companyService
    )
    {
        _hostEnvironment = hostEnvironment;
        _localization = localization;
        _companyService = companyService;
    }
    
    public async Task<IActionResult> Index()
    {
        var companies = await _companyService.GetCompaniesAsync();
        return View("Companies", companies);
    }

    public async Task<IActionResult> GetBranches(int companyId)
    {
        var branches = await _companyService.GetBranchesByCompanyIdAsync(companyId);
        return View("Index", branches);
    }
    
    public IActionResult DownloadDocs(DocTypeEnum docType)
    {
        var file = GetFileFromDocType(docType);

        if (file is null) return NotFound();
        
        return file;
    }

    #region Private Methods

    private FileStreamResult? GetFileFromDocType(DocTypeEnum docType)
    {
        var isRus = string.Equals(CultureInfo.CurrentCulture.Name, "ru", StringComparison.OrdinalIgnoreCase);
        var webRootPath = _hostEnvironment.WebRootPath;
        string downloadFileText;
        string filePath;

        switch (docType)
        {
            case DocTypeEnum.Privacy:
                filePath = Path.Combine(webRootPath, "documents\\privacy.pdf");
                downloadFileText = "Документ условия и соглашения.pdf";
                break;
            case DocTypeEnum.InstructionForQueue:
                filePath = Path.Combine(webRootPath, isRus ? "documents\\InstructionsRus.pdf" : "documents\\InstructionKG.pdf");
                downloadFileText = $"{_localization.GetLocalizedString("Email_request")}.pdf";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(docType), docType, @"Документ для скачивания не найден");
        }

        return System.IO.File.Exists(filePath) 
            ? File(System.IO.File.OpenRead(filePath), "application/pdf", downloadFileText) 
            : null;
    }

    #endregion
}