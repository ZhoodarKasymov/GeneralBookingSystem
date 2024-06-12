using BookingQueue.Common.Models.Company;
using Dapper.FluentMap.Mapping;

namespace BookingQueue.BLL.DapperMappers;

public class BranchMap : EntityMap<Branch>
{
    public BranchMap()
    {
        Map(b => b.IsProgress).ToColumn("is_progress");
        Map(b => b.CompanyId).ToColumn("company_id");
    }
}