namespace BookingQueue.DAL.GenericRepository;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(int id);
    Task<int> InsertAsync(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<T>> QueryDynamicAsync<T>(string query);
}