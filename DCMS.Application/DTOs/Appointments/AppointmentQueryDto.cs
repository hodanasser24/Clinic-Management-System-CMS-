using System;
using DCMS.Domain.Enums;

namespace DCMS.Application.DTOs.Appointments;

public class AppointmentQueryDto
{
    // Search
    public int? Id { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientPhone { get; set; }

    // Filters
    public AppointmentStatus? Status { get; set; }
    public int? DoctorId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

    // Sorting
    public string? SortBy { get; set; } // "CreatedAt" or "Date"
    public bool SortDescending { get; set; } = true; // Default to Descending

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
