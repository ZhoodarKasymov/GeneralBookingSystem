using BookingQueue.Common.Models.User;

namespace BookingQueue.BLL.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task CreateUserAsync(User user);
    Task<Role> GetRoleByNameAsync(string name);
    Task<User?> AuthenticateAsync(string username, string password);
}