using System.Collections.Generic;
using System.Threading.Tasks;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;

namespace DCMS.Infrastructure.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    public Task AddAsync(Schedule entity) => throw new System.NotImplementedException();
    public void Delete(Schedule entity) => throw new System.NotImplementedException();
    public Task<Schedule?> GetByIdAsync(int id) => throw new System.NotImplementedException();
    public Task<IReadOnlyList<Schedule>> ListAllAsync() => throw new System.NotImplementedException();
    public void Update(Schedule entity) => throw new System.NotImplementedException();
}
