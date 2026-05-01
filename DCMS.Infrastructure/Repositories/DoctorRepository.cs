using System.Collections.Generic;
using System.Threading.Tasks;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;

namespace DCMS.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    public Task AddAsync(Doctor entity) => throw new System.NotImplementedException();
    public void Delete(Doctor entity) => throw new System.NotImplementedException();
    public Task<Doctor?> GetByIdAsync(int id) => throw new System.NotImplementedException();
    public Task<IReadOnlyList<Doctor>> ListAllAsync() => throw new System.NotImplementedException();
    public void Update(Doctor entity) => throw new System.NotImplementedException();
}
