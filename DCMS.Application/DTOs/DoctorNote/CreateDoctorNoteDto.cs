using System.ComponentModel.DataAnnotations;

namespace DCMS.Application.DTOs.DoctorNote;

public class CreateDoctorNoteDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
