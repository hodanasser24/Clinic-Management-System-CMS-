using System;
using System.Threading.Tasks;
using DCMS.Domain.Entities;
using DCMS.Domain.Interfaces.Repositories;

namespace DCMS.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IPatientRepository Patients { get; }
    IDoctorRepository Doctors { get; }
    IAppointmentRepository Appointments { get; }
    IScheduleRepository Schedules { get; }
    IReportRepository Reports { get; }
    
    // Fallback for other entities if needed
    IGenericRepository<T> Repository<T>() where T : Common.BaseEntity;

    Task<int> SaveChangesAsync();
}
