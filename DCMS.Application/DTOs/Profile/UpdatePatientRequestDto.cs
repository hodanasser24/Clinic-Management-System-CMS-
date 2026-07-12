using System;

namespace DCMS.Application.DTOs.Profile;

public class UpdatePatientRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Patient specific fields
    public DateOnly DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }
    public string? BloodType { get; set; }
    public string? Gender { get; set; }
    public string? Allergies { get; set; }
}
