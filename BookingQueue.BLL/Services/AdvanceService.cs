using System.Data;
using BookingQueue.BLL.Helpers;
using BookingQueue.BLL.Resources;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using BookingQueue.Common.Models;
using BookingQueue.Common.Models.ViewModels;
using BookingQueue.DAL.GenericRepository;
using Dapper;

namespace BookingQueue.BLL.Services;

public class AdvanceService : IAdvanceService
{
    private readonly IDbConnection _db;
    private readonly IGenericRepository<Advance> _repository;
    private readonly LocService _localization;

    public AdvanceService(
        Func<string, IDbConnection> connectionFactory, 
        IGenericRepository<Advance> repository,
        LocService localization)
    {
        _db = connectionFactory(DatabaseConstants.SessionBased);
        _repository = repository;
        _localization = localization;
    }

    public async Task<string> BookTimeAsync(BookViewModel bookViewModel)
    {
        await CheckAdvanceDateTimeAsync(bookViewModel.BookingDate, bookViewModel.ServiceId);
        
        await CheckWorkingTimeAsync(bookViewModel.BookingDate);

        var uniqueNumber = await GenerateUniqueIDAsync();

        var result = await _repository.InsertAsync(new Advance
        {
            Id = uniqueNumber,
            Comments = $"{bookViewModel.Name}; {bookViewModel.PhoneNumber}",
            AdvanceTime = bookViewModel.BookingDate,
            Priority = 2,
            ServiceId = bookViewModel.ServiceId
        });

        if (result <= 0)
            throw new Exception(_localization.GetLocalizedString("Error_NotSaved"));

        return uniqueNumber.ToString();
    }

    #region Private Methods

    private async Task CheckAdvanceDateTimeAsync(DateTime? date, long? serviceId)
    {
        var query = @"SELECT 
                        CASE 
	                        WHEN (SELECT COUNT(*) FROM advance WHERE advance_time = @DateTime && service_id = s.id) >= s.advance_limit
                            THEN 'true'
	                        ELSE 'false' 
                        END AS is_more_then_advance_limit
                        FROM services AS s
                        WHERE s.id = @ServiveId;";
        
        var result = await _db.QueryFirstOrDefaultAsync(query, new { DateTime = date, ServiveId =  serviceId});
        
        if(Convert.ToBoolean(result.is_more_then_advance_limit))
            throw new Exception(_localization.GetLocalizedString("Error_MaxCountOfUser"));
    }

    private async Task CheckWorkingTimeAsync(DateTime? date)
    {
        var time = date.Value.TimeOfDay;
        
        if(DateTime.Now.Date > date.Value.Date || (DateTime.Now.Date == date.Value.Date && DateTime.Now.TimeOfDay > time))
            throw new Exception(_localization.GetLocalizedString("Error_NotWorking"));
        
        var dayOfWeekNumber = DateTimeHelpers.GetCorrectedTimeBegin(date.Value.DayOfWeek);
        
        var query = $@"SELECT
                      CASE
                        WHEN time(@time) BETWEEN time_begin_{dayOfWeekNumber} AND time_end_{dayOfWeekNumber} THEN 'false'
                        ELSE 'true'
                      END AS is_not_in_time_range
                    FROM schedule 
                    WHERE id = 1;";

        var result = await _db.QueryFirstOrDefaultAsync(query, new { time = time.ToString() });
        
        if(Convert.ToBoolean(result.is_not_in_time_range))
            throw new Exception(_localization.GetLocalizedString("Error_NotWorking"));
    }

    private async Task<long> GenerateUniqueIDAsync()
    {
        var id = GenerateRandomID();
        var maxValue = Convert.ToDecimal(id);
        
        if (maxValue > 2147483647) await GenerateUniqueIDAsync();
        
        while (!await IsUniqueIDAsync(id))
        {
            id = GenerateRandomID();
        }

        return Convert.ToInt64(id);
    }

    private string GenerateRandomID()
    {
        var random = new Random();
        
        var idLength = random.Next(5, 10);
        var chars = "0123456789";
        
        return new string(Enumerable.Repeat(chars, idLength)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task<bool> IsUniqueIDAsync(string id)
    {
        var query = $"SELECT COUNT(*) FROM {typeof(Advance).Name} WHERE id = @ID";
        var count = await _db.ExecuteScalarAsync<int>(query, new { ID = id });
        return count == 0;
    }

    #endregion
}