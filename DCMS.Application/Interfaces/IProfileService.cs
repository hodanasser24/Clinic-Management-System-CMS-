using DCMS.Application.DTOs.Profile;

namespace DCMS.Application.Interfaces;

public interface IProfileService
{
    Task<PatientProfileResponseDto> GetPatientProfileAsync(int patientId, CancellationToken ct = default);
    Task<PatientProfileResponseDto> UpdatePatientProfileAsync(int patientId, UpdatePatientProfileRequestDto dto, CancellationToken ct = default);
    Task<DoctorProfileResponseDto> GetDoctorProfileAsync(int doctorId, CancellationToken ct = default);
    Task<DoctorProfileResponseDto> UpdateDoctorSelfProfileAsync(int doctorId, UpdateDoctorSelfProfileRequestDto dto, CancellationToken ct = default);
    Task<DoctorProfileResponseDto> UpdateDoctorPhotoAsync(int doctorId, UpdatePhotoRequestDto dto, CancellationToken ct = default);
}
