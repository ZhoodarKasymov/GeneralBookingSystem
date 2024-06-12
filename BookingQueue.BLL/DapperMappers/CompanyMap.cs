using BookingQueue.Common.Models.Company;
using Dapper.FluentMap.Mapping;

namespace BookingQueue.BLL.DapperMappers;

public class CompanyMap : EntityMap<Company>
{
    public CompanyMap()
    {
        Map(u => u.IconPath).ToColumn("icon_path");
    }
}