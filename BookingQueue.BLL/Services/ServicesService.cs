using System.Data;
using BookingQueue.BLL.Helpers;
using BookingQueue.BLL.Resources;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using BookingQueue.DAL.GenericRepository;
using Dapper;

namespace BookingQueue.BLL.Services;

public class ServicesService : IServicesService
{
    private readonly IGenericRepository<Common.Models.Services> _genericRepository;
    private readonly IDbConnection _db;
    private readonly LocService _localization;

    public ServicesService(
        IGenericRepository<Common.Models.Services> genericRepository,
        Func<string, IDbConnection> connectionFactory,
        LocService localization)
    {
        _genericRepository = genericRepository;
        _db = connectionFactory(DatabaseConstants.SessionBased);
        _localization = localization;
    }

    public async Task<List<string>> GetTimeWithPeriodByDate(DateTime? date, long? serviceId)
    {
        await CheckDayOfCalendarAsync(date, serviceId);
        
        var dayOfWeekNumber = DateTimeHelpers.GetCorrectedTimeBegin(date.Value.DayOfWeek);

        return await GetTimeWithPeriodAsync(dayOfWeekNumber, serviceId);
    }

    public async Task<List<Common.Models.Services>> GetAllActiveAsync()
    {
        var query = @"SELECT s.id, s.name, s.prent_id,  sl.name as 'TranslatedName', s.deleted, sl.lang FROM services_langs sl
                        RIGHT JOIN services s ON s.id = sl.services_id
                        WHERE s.deleted IS NULL && sl.lang = 'kz_KZ'";

        var services = await _genericRepository.QueryDynamicAsync<Common.Models.Services>(query);
        return services.ToList();
    }

    #region PrivateMethods

    private async Task<List<string>> GetTimeWithPeriodAsync(int dayOfWeekNumber, long? serviceId)
    {
        var query = @$"SELECT s.advance_time_period, day_limit, sc.time_begin_{dayOfWeekNumber} as 'time_start', sc.time_end_{dayOfWeekNumber} as 'time_end'  FROM services s
                        INNER JOIN schedule sc ON sc.id = 1
                        WHERE s.id = @ServiveId;";
        var result = await _db.QueryFirstOrDefaultAsync(query, new { ServiveId = serviceId });
        
        var startTimeStr = result.time_start?.ToString();
        var endTimeStr = result.time_end?.ToString();
        var advanceTimePeriodStr = result.advance_time_period?.ToString();
        var dayLimitStr = result.day_limit?.ToString();
        
        if (startTimeStr is null || endTimeStr is null) throw new Exception(_localization.GetLocalizedString("Error_NotWorking"));

        var times = new List<string>();
        var startTime = TimeSpan.Parse(startTimeStr);
        var endTime = TimeSpan.Parse(endTimeStr);
        var interval = new TimeSpan(0, Convert.ToInt32(advanceTimePeriodStr ?? "0"), 0);
        
        for (var time = startTime; time <= endTime; time = time.Add(interval))
        {
            times.Add(time.ToString(@"hh\:mm"));
            
            if (times.Count == 2 && Convert.ToInt32(dayLimitStr ?? 0) != 0)
                interval = new TimeSpan(0, Convert.ToInt32(dayLimitStr ?? 0), 0);
        }

        return times;
    }

    private async Task CheckDayOfCalendarAsync(DateTime? date, long? serviceId)
    {
        var query = @"SELECT COUNT(*) FROM services s
                        INNER JOIN calendar_out_days cd on cd.calendar_id = s.calendar_id
                        WHERE s.id = @ServiveId && cd.out_day = @Date;";

        var count = await _db.ExecuteScalarAsync<int>(query, new { ServiveId = serviceId, Date = date.Value.Date });

        if (count > 0)
            throw new Exception(_localization.GetLocalizedString("Error_NotWorking"));
    }

    #endregion
}