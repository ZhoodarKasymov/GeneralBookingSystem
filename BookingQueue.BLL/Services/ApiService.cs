using System.Data;
using BookingQueue.BLL.Helpers;
using BookingQueue.BLL.Resources;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using BookingQueue.Common.Models.Company;
using BookingQueue.Common.Models.DTO;
using Dapper;

namespace BookingQueue.BLL.Services;

public class ApiService : IApiService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbConnection _dbConnection;
    private readonly LocService _localization;

    public ApiService(IDbConnectionFactory connectionFactory, Func<string, IDbConnection> connection, LocService localization)
    {
        _connectionFactory = connectionFactory;
        _dbConnection = connection(DatabaseConstants.Default);
        _localization = localization;
    }

    public async Task<List<ServiceDto>> GetServices(int branchId)
    {
        var conStr = GetConnectionByBranchId(branchId);

        using (var db = _connectionFactory.CreateConnection(conStr))
        {
            var query = @"SELECT s.id, s.name, s.prent_id,  sl.name as 'TranslatedName', s.deleted, sl.lang FROM services_langs sl
                        RIGHT JOIN services s ON s.id = sl.services_id
                        WHERE s.deleted IS NULL && sl.lang = 'kz_KZ'";

            var services = await db.QueryAsync<Common.Models.Services>(query);
            
            return GetSubServices("ru", 1, services.ToList());
        }
    }

    public async Task<List<string>> GetTimeWithPeriodByDate(int branchId, DateTime? date, long? serviceId)
    {
        var conStr = GetConnectionByBranchId(branchId);

        using (var db = _connectionFactory.CreateConnection(conStr))
        {
            await CheckDayOfCalendarAsync(db, date, serviceId);
        
            var dayOfWeekNumber = DateTimeHelpers.GetCorrectedTimeBegin(date.Value.DayOfWeek);

            return await GetTimeWithPeriodAsync(db, dayOfWeekNumber, serviceId);
        }
    }

    public async Task<string> BookingTime(int branchId, DateTime? bookingTime, long? serviceId)
    {
        var conStr = GetConnectionByBranchId(branchId);

        using (var db = _connectionFactory.CreateConnection(conStr))
        {
            await CheckAdvanceDateTimeAsync(db, bookingTime, serviceId);
        
            await CheckWorkingTimeAsync(db, bookingTime);

            var uniqueNumber = await GenerateUniqueIDAsync(db);

            var insertSql = @"INSERT INTO advance (id, advance_time, priority, service_id)
                            VALUES(@Id, @Date, @Priority, @ServiceId)";

            var result = await db.ExecuteAsync(insertSql, new
            {
                Id = uniqueNumber,
                Date = bookingTime,
                Priority = 2,
                ServiceId = serviceId
            });

            if (result <= 0)
                throw new Exception("Что-то пошло не так, очередь не сохранен.");

            return uniqueNumber.ToString();
        }
    }

    #region Private methods
    
    private List<ServiceDto> GetSubServices(string ci, long? id, List<Common.Models.Services> services)
    {
        var servicesDtos = new List<ServiceDto>();

        foreach (var mainSubService in services.Where(s => s.prent_id == id))
        {
            var serviceDto = new ServiceDto
            {
                ServiceId = mainSubService.Id.Value,
                ServiceName = ci == "uk" ? mainSubService.TranslatedName : mainSubService.Name,
                SubServices = GetSubServices(ci, mainSubService.Id, services)
            };

            servicesDtos.Add(serviceDto);
        }

        return servicesDtos;
    }

    private string GetConnectionByBranchId(int branchId)
    {
        var sql = "SELECT * FROM branches WHERE id = @BranchId";
        var branch = _dbConnection.QueryFirstOrDefault<Branch>(sql, new { BranchId = branchId });

        if (branch is null)
            throw new Exception("Филлиал не найден!");
        
        return branch.Connection;
    }
    
    private async Task<List<string>> GetTimeWithPeriodAsync(IDbConnection db, int dayOfWeekNumber, long? serviceId)
    {
        var query = @$"SELECT s.advance_time_period, day_limit, sc.time_begin_{dayOfWeekNumber} as 'time_start', sc.time_end_{dayOfWeekNumber} as 'time_end'  FROM services s
                        INNER JOIN schedule sc ON sc.id = 1
                        WHERE s.id = @ServiveId;";
        var result = await db.QueryFirstOrDefaultAsync(query, new { ServiveId = serviceId });
        
        var startTimeStr = result.time_start?.ToString();
        var endTimeStr = result.time_end?.ToString();
        var advanceTimePeriodStr = result.advance_time_period?.ToString();
        var dayLimitStr = result.day_limit?.ToString();
        
        if (startTimeStr is null || endTimeStr is null) throw new Exception("Не рабочий период времени!");

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

    private async Task CheckDayOfCalendarAsync(IDbConnection db, DateTime? date, long? serviceId)
    {
        var query = @"SELECT COUNT(*) FROM services s
                        INNER JOIN calendar_out_days cd on cd.calendar_id = s.calendar_id
                        WHERE s.id = @ServiveId && cd.out_day = @Date;";

        var count = await db.ExecuteScalarAsync<int>(query, new { ServiveId = serviceId, Date = date.Value.Date });

        if (count > 0)
            throw new Exception("Не рабочий период времени!");
    }
    
    private async Task CheckAdvanceDateTimeAsync(IDbConnection db, DateTime? date, long? serviceId)
    {
        var query = @"SELECT 
                        CASE 
	                        WHEN (SELECT COUNT(*) FROM advance WHERE advance_time = @DateTime && service_id = s.id) >= s.advance_limit
                            THEN 'true'
	                        ELSE 'false' 
                        END AS is_more_then_advance_limit
                        FROM services AS s
                        WHERE s.id = @ServiveId;";
        
        var result = await db.QueryFirstOrDefaultAsync(query, new { DateTime = date, ServiveId =  serviceId});
        
        if(Convert.ToBoolean(result.is_more_then_advance_limit))
            throw new Exception("Нет свободного места, выберите другое время!");
    }

    private async Task CheckWorkingTimeAsync(IDbConnection db, DateTime? date)
    {
        var time = date.Value.TimeOfDay;
        
        if(DateTime.Now.Date > date.Value.Date || (DateTime.Now.Date == date.Value.Date && DateTime.Now.TimeOfDay > time))
            throw new Exception("Не рабочий период времени!");
        
        var dayOfWeekNumber = DateTimeHelpers.GetCorrectedTimeBegin(date.Value.DayOfWeek);
        
        var query = $@"SELECT
                      CASE
                        WHEN time(@time) BETWEEN time_begin_{dayOfWeekNumber} AND time_end_{dayOfWeekNumber} THEN 'false'
                        ELSE 'true'
                      END AS is_not_in_time_range
                    FROM schedule 
                    WHERE id = 1;";

        var result = await db.QueryFirstOrDefaultAsync(query, new { time = time.ToString() });
        
        if(Convert.ToBoolean(result.is_not_in_time_range))
            throw new Exception("Не рабочий период времени!");
    }

    private async Task<long> GenerateUniqueIDAsync(IDbConnection db)
    {
        var id = GenerateRandomID();
        var maxValue = Convert.ToDecimal(id);
        
        if (maxValue > 2147483647) await GenerateUniqueIDAsync(db);
        
        while (!await IsUniqueIDAsync(db, id))
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

    private async Task<bool> IsUniqueIDAsync(IDbConnection db, string id)
    {
        var query = $"SELECT COUNT(*) FROM advance WHERE id = @ID";
        var count = await db.ExecuteScalarAsync<int>(query, new { ID = id });
        return count == 0;
    }

    #endregion
}