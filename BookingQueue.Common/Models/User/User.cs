
namespace BookingQueue.Common.Models.User;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public List<Role> Roles { get; set; }
    public int? CompanyId { get; set; }
    public Company.Company Company { get; set; }
}