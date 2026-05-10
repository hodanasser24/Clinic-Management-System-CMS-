namespace DCMS.Application.DTOs.Profile;

public class UpdatePatientProfileRequestDto
{
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }
}

public class UpdateDoctorSelfProfileRequestDto
{
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Specialization { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
}

public class UpdatePhotoRequestDto
{
    public string PhotoUrl { get; set; } = null!;
}

public class PatientProfileResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }
    public bool IsFirstLogin { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DoctorProfileResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Specialization { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public int ExperienceYears { get; set; }
    public bool IsFirstLogin { get; set; }
    public DateTime CreatedAt { get; set; }
}
