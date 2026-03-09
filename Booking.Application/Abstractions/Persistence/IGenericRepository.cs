namespace Booking.Application.Abstractions.Persistence;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken);

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}