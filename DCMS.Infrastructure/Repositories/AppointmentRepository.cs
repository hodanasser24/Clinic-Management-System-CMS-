using System.Collections.Generic;
using System.Threading.Tasks;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;

namespace DCMS.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    public Task AddAsync(Appointment entity) => throw new System.NotImplementedException();
    public void Delete(Appointment entity) => throw new System.NotImplementedException();
    public Task<Appointment?> GetByIdAsync(int id) => throw new System.NotImplementedException();
    public Task<IReadOnlyList<Appointment>> ListAllAsync() => throw new System.NotImplementedException();
    public void Update(Appointment entity) => throw new System.NotImplementedException();
}
