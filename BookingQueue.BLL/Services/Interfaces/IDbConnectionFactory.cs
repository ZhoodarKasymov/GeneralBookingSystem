using System.Data;

namespace BookingQueue.BLL.Services.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection(string connectionString);
}