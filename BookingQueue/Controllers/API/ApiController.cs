using System.Globalization;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Models.Company;
using BookingQueue.Common.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BookingQueue.Controllers.API;

[ApiController]
public class ApiController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly IApiService _apiService;

    public ApiController(ICompanyService companyService, IApiService apiService)
    {
        _companyService = companyService;
        _apiService = apiService;
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

    /// <summary>
    /// Для получения списка услуг по ID филиала
    /// </summary>
    /// <returns>Список услуг</returns>
    [HttpGet("/api/branches/{branchId}/services")]
    public async Task<List<ServiceDto>> GetServices(int? branchId)
    {
        if (branchId is null) throw new Exception("Branch ID не должна быть пустым!");
        
        return await _apiService.GetServices(branchId.Value);
    }

    /// <summary>
    /// Для получения списка доступного времени
    /// </summary>
    /// <param name="branchId">ID филиала</param>
    /// <param name="date">Дата без времени формат: dd/MM/yyyy</param>
    /// <param name="serviceId">ID услуги</param>
    /// <returns>Список времени</returns>
    [HttpGet("/api/branches/{branchId}/get-times")]
    public async Task<IEnumerable<string>> GetTimesByService(int? branchId, string? date, long? serviceId)
    {
        if (branchId is null) throw new Exception("Branch ID не должна быть пустым!");
        if (serviceId is null) throw new Exception("Service ID не должна быть пустым!");
        if (date is null) throw new Exception("Date не должна быть пустым!");
        
        var parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        return await _apiService.GetTimeWithPeriodByDate(branchId.Value, parsedDate, serviceId);
    }

    /// <summary>
    /// Для бронирования дня
    /// </summary>
    /// <param name="branchId">ID филиала</param>
    /// <param name="requestDto">Body обьект для бронирования: DateTime формат: dd/MM/yyyy HH:mm и ID услуги</param>
    /// <returns>Код талона</returns>
    [HttpPost("/api/branches/{branchId}/booking")]
    public async Task<string> BookingDay(int? branchId, [FromBody]BookingRequestDto requestDto)
    {
        if (branchId is null) throw new Exception("Branch ID не должна быть пустым!");
        if (requestDto.ServiceId is null) throw new Exception("Service ID не должна быть пустым!");
        if (requestDto.DateTime is null) throw new Exception("DateTime не должна быть пустым!");
        
        var parsedDate = DateTime.ParseExact(requestDto.DateTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        return await _apiService.BookingTime(branchId.Value, parsedDate, requestDto.ServiceId);
    }
}