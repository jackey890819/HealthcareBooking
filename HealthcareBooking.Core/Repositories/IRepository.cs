namespace HealthcareBooking.Core.Repositories;

public interface IRepository<TEntity>
{
    Task AddAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(int id);
}
