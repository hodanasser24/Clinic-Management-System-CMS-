using AutoMapper;
using DCMS.Application.DTOs;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;

namespace DCMS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── ContactMessage ────────────────────────────────────────────────────
        CreateMap<ContactMessage, ContactMessageResponse>()
            .ForMember(d => d.TypeLabel,   o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.RepliedBy,   o => o.MapFrom(s =>
                s.RepliedByUser == null
                    ? null
                    : s.RepliedByUser.FullName));

        CreateMap<ContactMessage, ContactMessageSummaryResponse>()
            .ForMember(d => d.TypeLabel,   o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.HasReply,    o => o.MapFrom(s => s.ReplyBody != null));

        CreateMap<CreateContactMessageRequest, ContactMessage>()
            .ForMember(d => d.SenderEmail, o => o.MapFrom(s => s.SenderEmail.Trim().ToLower()))
            .ForMember(d => d.SenderName,  o => o.MapFrom(s => s.SenderName.Trim()))
            .ForMember(d => d.Subject,     o => o.MapFrom(s => s.Subject.Trim()))
            .ForMember(d => d.Body,        o => o.MapFrom(s => s.Body.Trim()))
            .ForMember(d => d.Status,      o => o.MapFrom(_ => ContactMessageStatus.Pending))
            .ForMember(d => d.IsArchived,  o => o.MapFrom(_ => false));

        // ── Appointment ───────────────────────────────────────────────────────
        CreateMap<Appointment, AppointmentResponse>()
            .ForMember(d => d.PatientName,     o => o.MapFrom(s => s.Patient == null ? "" : s.Patient.FullName))
            .ForMember(d => d.DoctorName,      o => o.MapFrom(s => s.Doctor == null ? "" : s.Doctor.FullName))
            .ForMember(d => d.ServiceName,     o => o.MapFrom(s => s.Service == null ? "" : s.Service.Name))
            .ForMember(d => d.BranchName,      o => o.MapFrom(s => s.Branch == null ? "" : s.Branch.Name))
            .ForMember(d => d.StatusLabel,     o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.IsUrgent,        o => o.MapFrom(s => s.IsUrgent));

        CreateMap<CreateAppointmentRequest, Appointment>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => AppointmentStatus.Pending))
            .ForMember(d => d.IsUrgent, o => o.MapFrom(_ => false));

        // ── ApplicationUser / Doctor / Patient ────────────────────────────────
        CreateMap<User, UserSummaryResponse>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.RoleLabel, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<User, DoctorProfileResponse>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));

        CreateMap<User, PatientProfileResponse>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));

        CreateMap<UpdateDoctorProfileRequest, User>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));

        CreateMap<UpdatePatientProfileRequest, User>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));

        // ── Schedule ──────────────────────────────────────────────────────────
        CreateMap<Schedule, ScheduleResponse>()
            .ForMember(d => d.DoctorName, o => o.MapFrom(s =>
                s.Doctor == null ? "" : s.Doctor.FullName))
            .ForMember(d => d.BranchName, o => o.MapFrom(s =>
                s.Branch == null ? "" : s.Branch.Name))
            .ForMember(d => d.DayLabel,   o => o.MapFrom(s => s.DayOfWeek.ToString()));

        CreateMap<CreateScheduleRequest, Schedule>();

        CreateMap<ScheduleChangeRequest, ScheduleChangeRequestResponse>()
            .ForMember(d => d.DoctorName, o => o.MapFrom(s =>
                s.Doctor == null ? "" : s.Doctor.FullName))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()));

        // ── Service ───────────────────────────────────────────────────────────
        CreateMap<Service, ServiceResponse>()
            .ForMember(d => d.CategoryLabel, o => o.MapFrom(s => s.Category.ToString()));

        CreateMap<CreateServiceRequest, Service>();
        CreateMap<UpdateServiceRequest, Service>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));

        // ── ServiceModificationRequest ─────────────────────────────────────────
        CreateMap<ServiceModificationRequest, ServiceModificationRequestResponseDto>()
            .ForMember(d => d.ProposedName,  o => o.MapFrom(s => s.ProposedName))
            // Adjust to proper DTO map later if needed. The provided map used wrong DTO names.
            ;

        // ── Report ────────────────────────────────────────────────────────────
        CreateMap<Report, ReportResponse>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                s.Patient == null ? "" : s.Patient.FullName))
            .ForMember(d => d.DoctorName,  o => o.MapFrom(s =>
                s.Doctor  == null ? "" : s.Doctor.FullName));

        CreateMap<CreateReportRequestDto,  Report>();
        CreateMap<UpdateReportRequestDto,  Report>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));

        CreateMap<ToothRecord, ToothRecordResponseDto>()
            .ForMember(d => d.ToothStatus, o => o.MapFrom(s => s.ToothStatus));

        // ── Prescription ──────────────────────────────────────────────────────
        CreateMap<Prescription, PrescriptionResponse>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                s.Patient == null ? "" : s.Patient.FullName))
            .ForMember(d => d.DoctorName,  o => o.MapFrom(s =>
                s.Doctor  == null ? "" : s.Doctor.FullName));

        CreateMap<PrescriptionItem, PrescriptionItemResponse>();
        CreateMap<CreatePrescriptionRequest, Prescription>();
        CreateMap<PrescriptionItemRequest, PrescriptionItem>();

        // ── Notification ──────────────────────────────────────────────────────
        CreateMap<Notification, NotificationResponse>()
            .ForMember(d => d.TypeLabel, o => o.MapFrom(s => s.Type.ToString()));

        // ── Branch ────────────────────────────────────────────────────────────
        CreateMap<Branch, BranchResponse>();
        CreateMap<CreateBranchRequest, Branch>();
        CreateMap<UpdateBranchRequest, Branch>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));

        // ── Offer ─────────────────────────────────────────────────────────────
        CreateMap<OfferDiscount, OfferResponse>()
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.IsActive ? "Active" : "Inactive"));

        CreateMap<CreateOfferRequest,  OfferDiscount>();
        CreateMap<UpdateOfferRequest,  OfferDiscount>()
            .ForAllMembers(o => o.Condition((_, _, srcMember) => srcMember != null));

        // ── FAQ ───────────────────────────────────────────────────────────────
        CreateMap<FAQ, FaqResponse>();
        CreateMap<CreateFaqRequest, FAQ>();

        // ── SystemLog ─────────────────────────────────────────────────────────
        CreateMap<SystemLog, SystemLogResponseDto>();

        // ── Revenue ───────────────────────────────────────────────────────────
        CreateMap<Revenue, RevenueEntryResponse>()
            .ForMember(d => d.AppointmentId, o => o.MapFrom(s => s.AppointmentId))
            .ForMember(d => d.ServiceName,   o => o.MapFrom(s =>
                s.Appointment != null && s.Appointment.Service != null
                    ? s.Appointment.Service.Name : ""));
    }
}
