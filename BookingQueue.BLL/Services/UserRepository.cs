using System.Data;
using System.Security.Cryptography;
using System.Text;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using BookingQueue.Common.Models.Company;
using BookingQueue.Common.Models.User;
using Dapper;

namespace BookingQueue.BLL.Services;

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _dbConnection;

    public UserRepository(Func<string, IDbConnection> connectionFactory)
    {
        _dbConnection = connectionFactory(DatabaseConstants.Default);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var sql = @"
            SELECT * FROM users WHERE username = @Username;
            SELECT roles.* FROM roles
            INNER JOIN user_roles ON roles.id = user_roles.role_id
            WHERE user_roles.user_id = (SELECT id FROM users WHERE username = @Username);
            SELECT * FROM companies WHERE id = (SELECT company_id FROM users WHERE username = @Username)";

        await using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Username = username });
        var user = await multi.ReadFirstOrDefaultAsync<User>();
        
        if (user is not null)
        {
            user.Roles = (await multi.ReadAsync<Role>()).ToList();
            user.Company = await multi.ReadSingleOrDefaultAsync<Company>();
        }
        
        return user;
    }

    public async Task CreateUserAsync(User user)
    {
        var sql = "INSERT INTO users (username, password_hash, company_id) VALUES (@Username, @PasswordHash, @CompanyId);SELECT LAST_INSERT_ID();";
        var userId = await _dbConnection.QuerySingleAsync<int>(sql, user);

        var role = await GetRoleByNameAsync(RoleConstants.Admin);
        
        await _dbConnection.ExecuteAsync("INSERT INTO user_roles (user_id, role_id) VALUES (@UserId, @RoleId);", new { UserId = userId, RoleId = role.Id });
    }

    public async Task<Role> GetRoleByNameAsync(string name)
    {
        return await _dbConnection.QuerySingleOrDefaultAsync<Role>("SELECT * FROM roles WHERE name = @Name;", new { Name = name });
    }
    
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await GetUserByUsernameAsync(username);

        if (user is null) return null;
        
        var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
        var passwordHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

        return user.PasswordHash == passwordHash ? user : null;
    }
}