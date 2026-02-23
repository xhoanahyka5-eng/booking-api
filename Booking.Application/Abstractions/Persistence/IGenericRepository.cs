using System.Linq.Expressions;

namespace Booking.Application.Abstractions.Persistence;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken);

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    void Update(TEntity entity);

    void Delete(TEntity entity);
}