using System.Collections.Generic;
using System.Threading.Tasks;
using DCMS.Domain.Common;

namespace DCMS.Domain.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
