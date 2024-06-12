using System.Data;
using BookingQueue.BLL.Services.Interfaces;
using MySql.Data.MySqlClient;

namespace BookingQueue.BLL.Services;

public class DbConnectionFactory : IDbConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }
}