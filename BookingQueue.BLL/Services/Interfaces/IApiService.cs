using BookingQueue.Common.Models.DTO;

namespace BookingQueue.BLL.Services.Interfaces;

public interface IApiService
{
    Task<List<ServiceDto>> GetServices(int branchId);

    Task<List<string>> GetTimeWithPeriodByDate(int branchId, DateTime? date, long? serviceId);

    Task<string> BookingTime(int branchId, DateTime? bookingTime, long? serviceId);
}