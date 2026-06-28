namespace DCMS.Application.DTOs.Profile;

public class PatientQueryDto
{
    // Search
    public int? Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }

    // Filters
    public int? BranchId { get; set; }
    public int? ServiceId { get; set; }

    // Sorting
    public string? SortBy { get; set; } // "Registered" or "Name"
    public bool SortDescending { get; set; } = true;

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
