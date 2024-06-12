using BookingQueue.Common.Models.User;
using Dapper.FluentMap.Mapping;

namespace BookingQueue.BLL.DapperMappers;

public class UserMap : EntityMap<User>
{
    public UserMap()
    {
        Map(u => u.Id).ToColumn("Id");
        Map(u => u.Username).ToColumn("Username");
        Map(u => u.PasswordHash).ToColumn("password_hash");
        Map(u => u.CompanyId).ToColumn("company_id");
    }
}