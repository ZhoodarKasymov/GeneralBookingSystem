using BookingQueue.Common.Models.ViewModels;

namespace BookingQueue.BLL.Services.Interfaces;

public interface IAdvanceService
{
    Task<string> BookTimeAsync(BookViewModel bookViewModel);
}