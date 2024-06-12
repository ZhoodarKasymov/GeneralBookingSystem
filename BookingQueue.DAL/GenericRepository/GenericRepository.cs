using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using BookingQueue.Common.Constants;
using Dapper;

namespace BookingQueue.DAL.GenericRepository;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly IDbConnection _db;

    public GenericRepository(Func<string, IDbConnection> connectionFactory)
    {
        _db = connectionFactory(DatabaseConstants.SessionBased);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        string query = $"SELECT * FROM {typeof(TEntity).Name}";
        return await _db.QueryAsync<TEntity>(query);
    }

    public async Task<TEntity> GetByIdAsync(int id)
    {
        var query = $"SELECT * FROM {typeof(TEntity).Name} WHERE id = @Id";
        return await _db.QueryFirstOrDefaultAsync<TEntity>(query, new { Id = id });
    }

    public async Task<int> InsertAsync(TEntity entity)
    {
        var query = GenerateInsertQuery();
        return await _db.ExecuteAsync(query, entity);
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        var query = GenerateUpdateQuery();
        var rowsAffected = await _db.ExecuteAsync(query, entity);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        string query = $"DELETE FROM {typeof(TEntity).Name} WHERE id = @Id";
        int rowsAffected = await _db.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<T>> QueryDynamicAsync<T>(string query)
    {
        return await _db.QueryAsync<T>(query);
    }

    #region Private methods

    private string GenerateInsertQuery()
    {
        var tableName = typeof(TEntity).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TEntity).Name;
        var properties = typeof(TEntity).GetProperties();
        var columns = string.Join(",", properties.Select(p => p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name));
        var values = string.Join(",", properties.Select(p => "@" + p.Name));
        return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
    }

    private string GenerateUpdateQuery()
    {
        var tableName = typeof(TEntity).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(TEntity).Name;
        var properties = typeof(TEntity).GetProperties().Where(p => p.Name != "Id");
        var columns = string.Join(",", properties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name}=@{p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name}"));
        return $"UPDATE {tableName} SET {columns} WHERE Id=@Id";
    }

    #endregion
}