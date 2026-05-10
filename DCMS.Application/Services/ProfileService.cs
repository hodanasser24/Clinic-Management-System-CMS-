using DCMS.Application.DTOs.Profile;
using DCMS.Application.Exceptions;
using DCMS.Application.Interfaces;
using DCMS.Domain.Interfaces;

namespace DCMS.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _uow;

    public ProfileService(IUnitOfWork uow) => _uow = uow;

    public async Task<PatientProfileResponseDto> GetPatientProfileAsync(int patientId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient == null) throw new NotFoundException($"Patient {patientId} not found.");
        return new PatientProfileResponseDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Email = patient.Email,
            Phone = patient.Phone,
            DateOfBirth = patient.DateOfBirth,
            MedicalHistory = patient.MedicalHistory,
            IsFirstLogin = patient.IsFirstLogin,
            CreatedAt = patient.CreatedAt
        };
    }

    public async Task<PatientProfileResponseDto> UpdatePatientProfileAsync(int patientId, UpdatePatientProfileRequestDto dto, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct);
        if (patient == null) throw new NotFoundException($"Patient {patientId} not found.");

        patient.FullName = dto.FullName;
        patient.Phone = dto.Phone;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.MedicalHistory = dto.MedicalHistory;

        await _uow.SaveChangesAsync(ct);
        return new PatientProfileResponseDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Email = patient.Email,
            Phone = patient.Phone,
            DateOfBirth = patient.DateOfBirth,
            MedicalHistory = patient.MedicalHistory,
            IsFirstLogin = patient.IsFirstLogin,
            CreatedAt = patient.CreatedAt
        };
    }

    public async Task<DoctorProfileResponseDto> GetDoctorProfileAsync(int doctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        if (doctor == null) throw new NotFoundException($"Doctor {doctorId} not found.");
        return MapDoctorProfile(doctor);
    }

    public async Task<DoctorProfileResponseDto> UpdateDoctorSelfProfileAsync(int doctorId, UpdateDoctorSelfProfileRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        if (doctor == null) throw new NotFoundException($"Doctor {doctorId} not found.");

        doctor.FullName = dto.FullName;
        doctor.Phone = dto.Phone;
        doctor.Specialization = dto.Specialization;
        doctor.Qualification = dto.Qualification;
        doctor.Bio = dto.Bio;
        doctor.ExperienceYears = dto.ExperienceYears;

        await _uow.SaveChangesAsync(ct);
        return MapDoctorProfile(doctor);
    }

    public async Task<DoctorProfileResponseDto> UpdateDoctorPhotoAsync(int doctorId, UpdatePhotoRequestDto dto, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        if (doctor == null) throw new NotFoundException($"Doctor {doctorId} not found.");

        doctor.PhotoUrl = dto.PhotoUrl;
        await _uow.SaveChangesAsync(ct);
        return MapDoctorProfile(doctor);
    }

    private static DoctorProfileResponseDto MapDoctorProfile(DCMS.Domain.Entities.Doctor d) => new()
    {
        Id = d.Id,
        FullName = d.FullName,
        Email = d.Email,
        Phone = d.Phone,
        Specialization = d.Specialization,
        Qualification = d.Qualification,
        Bio = d.Bio,
        PhotoUrl = d.PhotoUrl,
        ExperienceYears = d.ExperienceYears,
        IsFirstLogin = d.IsFirstLogin,
        CreatedAt = d.CreatedAt
    };
}
