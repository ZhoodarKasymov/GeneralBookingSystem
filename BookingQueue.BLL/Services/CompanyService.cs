using System.Data;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using BookingQueue.Common.Models.Company;
using BookingQueue.Common.Models.User;
using Dapper;

namespace BookingQueue.BLL.Services;

public class CompanyService : ICompanyService
{
    private readonly IDbConnection _dbConnection;

    public CompanyService(Func<string, IDbConnection> connectionFactory)
    {
        _dbConnection = connectionFactory(DatabaseConstants.Default);
    }

    public async Task<IEnumerable<Branch>> GetBranchesByCompanyIdAsync(int companyId)
    {
        var sql = @"SELECT * FROM branches WHERE company_id = @CompanyId;";
        var branches = await _dbConnection.QueryAsync<Branch>(sql, new { CompanyId = companyId });
        return branches;
    }

    public async Task<Company?> GetCompanyByIdAsync(int id)
    {
        var sql = @"
            SELECT * FROM companies WHERE Id = @CompanyId AND deletedAt IS NULL;
            SELECT * FROM branches WHERE company_id = @CompanyId;";
        
        await using var multi = await _dbConnection.QueryMultipleAsync(sql, new { CompanyId = id });
        var company = await multi.ReadFirstOrDefaultAsync<Company>();

        if (company is null) throw new NoNullAllowedException("Компания не найдена или была удалена!");
        
        var branches = await multi.ReadAsync<Branch>();

        company.Branches = branches.ToList();

        return company;
    }

    public async Task CreateOrUpdateCompanyAsync(Company company)
    {
        if (company.Id == 0)
        {
            // Create a new company
            var sqlInsert = @"
                            INSERT INTO companies (name, title, icon_path, companyLink, companyPhone, companyMail) 
                            VALUES (@Name, @Title, @IconPath, @CompanyLink, @CompanyPhone, @CompanyMail);
                            SELECT LAST_INSERT_ID();";
            var companyId = await _dbConnection.QuerySingleAsync<int>(sqlInsert, company);
            company.Id = companyId;
        }
        else
        {
            // Update an existing company
            var sqlUpdate = @"
                            UPDATE companies 
                            SET name = @Name, 
                                title = @Title, 
                                icon_path = @IconPath, 
                                companyLink = @CompanyLink, 
                                companyPhone = @CompanyPhone, 
                                companyMail = @CompanyMail 
                            WHERE id = @Id;";
            await _dbConnection.ExecuteAsync(sqlUpdate, company);
        }
    }

    public async Task CreateOrUpdateBranchesAsync(int companyId, List<Branch> branches)
    {
        var deleteSql = "DELETE FROM branches WHERE company_id = @CompanyId;";
    
        // Ensure the connection is open
        if (_dbConnection.State != ConnectionState.Open)
        {
            _dbConnection.Open();
        }

        using var transaction = _dbConnection.BeginTransaction();

        try
        {
            // Delete existing branches
            await _dbConnection.ExecuteAsync(deleteSql, new { CompanyId = companyId }, transaction);

            var insertSql =
                "INSERT INTO branches (company_id, name, connection, is_progress, address) VALUES (@CompanyId, @Name, @Connection, @IsProgress, @Address);";

            // Insert new branches
            foreach (var branch in branches)
            {
                await _dbConnection.ExecuteAsync(insertSql, branch, transaction);
            }

            // Commit transaction if successful
            transaction.Commit();
        }
        catch
        {
            // Rollback transaction if an exception occurs
            transaction.Rollback();
            throw;
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<IEnumerable<Company>> GetCompaniesAsync()
    {
        var sql = @"SELECT * FROM companies WHERE deletedAt IS NULL";
        var companies = await _dbConnection.QueryAsync<Company>(sql);
        return companies;
    }
    
    public async Task<(IEnumerable<Company> Companies, int TotalCount)> GetCompaniesAsync(int pageNumber, int pageSize)
    {
        var sql = @"
            SELECT SQL_CALC_FOUND_ROWS 
                c.*,
                u.id AS UserId, u.username, u.password_hash, u.company_id
            FROM companies c
            LEFT JOIN users u ON c.id = u.company_id
            WHERE c.deletedAt IS NULL
            ORDER BY c.id
            LIMIT @PageSize OFFSET @Offset;
            SELECT FOUND_ROWS();";

        var offset = (pageNumber - 1) * pageSize;

        using (var multi = await _dbConnection.QueryMultipleAsync(sql, new { PageSize = pageSize, Offset = offset }))
        {
            var companies = multi.Read<Company, User, Company>(
                (company, user) =>
                {
                    company.User = user;
                    return company;
                }, splitOn: "UserId").ToList();

            var totalCount = multi.Read<int>().Single();

            return (companies, totalCount);
        }
    }
    
    public async Task<IEnumerable<Company>> GetAllCompaniesWithBranchesAsync()
    {
        var sql = @"
            SELECT 
                c.*, 
                b.id AS BranchId, b.id, b.name, b.connection, b.is_progress, b.address
            FROM 
                companies c
            LEFT JOIN 
                branches b ON c.id = b.company_id
            WHERE c.deletedAt IS NULL
            ORDER BY 
                c.id, b.id;";

        var companyDict = new Dictionary<int, Company>();

        var result = await _dbConnection.QueryAsync<Company, Branch, Company>(
            sql,
            (company, branch) =>
            {
                if (!companyDict.TryGetValue(company.Id, out var currentCompany))
                {
                    currentCompany = company;
                    currentCompany.Branches = new List<Branch>();
                    companyDict.Add(currentCompany.Id, currentCompany);
                }

                branch.CompanyId = company.Id;
                currentCompany.Branches.Add(branch);
                return currentCompany;
            },
            splitOn: "BranchId"
        );

        return companyDict.Values;
    }

    public async Task DeleteCompany(int companyId)
    {
        var sql = @"UPDATE companies
                    SET deletedAt = now()
                    WHERE id = @CompanyId;";

        var result = await _dbConnection.ExecuteAsync(sql, new { CompanyId = companyId });
    }
}