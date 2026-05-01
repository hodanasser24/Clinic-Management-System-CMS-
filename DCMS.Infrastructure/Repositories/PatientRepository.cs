using System.Collections.Generic;
using System.Threading.Tasks;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;

namespace DCMS.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    public Task AddAsync(Patient entity) => throw new System.NotImplementedException();
    public void Delete(Patient entity) => throw new System.NotImplementedException();
    public Task<Patient?> GetByIdAsync(int id) => throw new System.NotImplementedException();
    public Task<IReadOnlyList<Patient>> ListAllAsync() => throw new System.NotImplementedException();
    public void Update(Patient entity) => throw new System.NotImplementedException();
}
