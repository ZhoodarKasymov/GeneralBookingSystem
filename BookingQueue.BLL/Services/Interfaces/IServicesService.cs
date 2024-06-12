namespace BookingQueue.BLL.Services.Interfaces;

public interface IServicesService
{
    Task<List<Common.Models.Services>> GetAllActiveAsync();

    Task<List<string>> GetTimeWithPeriodByDate(DateTime? date, long? serviceId);
}