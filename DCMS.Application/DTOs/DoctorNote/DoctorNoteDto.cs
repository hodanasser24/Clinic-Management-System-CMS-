namespace DCMS.Application.DTOs.DoctorNote;

public class DoctorNoteDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
