using AutoMapper;
using DCMS.Application.DTOs.Appointments;
using DCMS.Application.DTOs.Branches;
using DCMS.Application.DTOs.Contacts;
using DCMS.Application.DTOs.DentalChart;
using DCMS.Application.DTOs.FAQs;
using DCMS.Application.DTOs.ModificationRequests;
using DCMS.Application.DTOs.Notifications;
using DCMS.Application.DTOs.Offers;
using DCMS.Application.DTOs.Owner;
using DCMS.Application.DTOs.Prescriptions;
using DCMS.Application.DTOs.Profile;
using DCMS.Application.DTOs.Reports;
using DCMS.Application.DTOs.Schedules;
using DCMS.Application.DTOs.Services;
using DCMS.Application.DTOs.SystemLogs;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;

namespace DCMS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── ContactMessage ────────────────────────────────────────────────────
        CreateMap<ContactMessage, ContactMessageResponseDto>()
            .ForMember(d => d.TypeLabel,   o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.RepliedBy,   o => o.MapFrom(s =>
                s.RepliedByUser == null ? null : s.RepliedByUser.FullName))
            .ForMember(d => d.SenderType,  o => o.MapFrom(s => s.SenderType))
            .ForMember(d => d.SenderId,    o => o.MapFrom(s => s.SenderId));

        CreateMap<ContactMessage, ContactMessageSummaryResponseDto>()
            .ForMember(d => d.TypeLabel,   o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.HasReply,    o => o.MapFrom(s => s.ReplyBody != null))
            .ForMember(d => d.SenderType,  o => o.MapFrom(s => s.SenderType));

        CreateMap<CreateContactMessageRequestDto, ContactMessage>()
            .ForMember(d => d.SenderEmail,  o => o.MapFrom(s => s.SenderEmail.Trim().ToLower()))
            .ForMember(d => d.SenderName,   o => o.MapFrom(s => s.SenderName.Trim()))
            .ForMember(d => d.Subject,      o => o.MapFrom(s => s.Subject.Trim()))
            .ForMember(d => d.Body,         o => o.MapFrom(s => s.Body.Trim()))
            .ForMember(d => d.Status,       o => o.MapFrom(_ => ContactMessageStatus.New))
            .ForMember(d => d.IsArchived,   o => o.MapFrom(_ => false))
            // SenderType and SenderId are set by the service, not AutoMapper
            .ForMember(d => d.SenderType,   o => o.Ignore())
            .ForMember(d => d.SenderId,     o => o.Ignore())
            .ForMember(d => d.GuestSessionId, o => o.Ignore());

        // ── Appointment ───────────────────────────────────────────────────────
        CreateMap<Appointment, AppointmentResponseDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient == null ? "" : s.Patient.FullName))
            .ForMember(d => d.DoctorName,  o => o.MapFrom(s => s.Doctor  == null ? "" : s.Doctor.FullName))
            .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service == null ? "" : s.Service.Name))
            .ForMember(d => d.BranchName,  o => o.MapFrom(s => s.Branch  == null ? "" : s.Branch.Name));

        CreateMap<Appointment, AppointmentSummaryDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient == null ? "" : s.Patient.FullName))
            .ForMember(d => d.DoctorName,  o => o.MapFrom(s => s.Doctor  == null ? "" : s.Doctor.FullName))
            .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service == null ? "" : s.Service.Name))
            .ForMember(d => d.BranchName,  o => o.MapFrom(s => s.Branch  == null ? "" : s.Branch.Name));

        CreateMap<AppointmentRequestDto, Appointment>()
            .ForMember(d => d.Status,   o => o.MapFrom(_ => AppointmentStatus.Pending))
            .ForMember(d => d.IsUrgent, o => o.MapFrom(_ => false));

        // ── User / Doctor / Patient / Admin / Owner ───────────────────────────
        CreateMap<User,    AccountResponseDto>();
        CreateMap<Doctor,  DoctorAccountResponseDto>();
        CreateMap<Doctor,  DoctorProfileResponseDto>();
        CreateMap<Patient, PatientProfileResponseDto>();

        CreateMap<UpdateDoctorSelfProfileRequestDto, Doctor>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));
        CreateMap<UpdatePatientProfileRequestDto, Patient>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));
        CreateMap<UpdateDoctorProfileRequestDto, Doctor>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));

        // ── Schedule ──────────────────────────────────────────────────────────
        CreateMap<Schedule, ScheduleResponseDto>()
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor == null ? "" : s.Doctor.FullName))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch == null ? "" : s.Branch.Name));

        CreateMap<CreateScheduleRequestDto, Schedule>();

        CreateMap<ScheduleChangeRequest, ScheduleChangeRequestResponseDto>()
            .ForMember(d => d.RequestingDoctorName, o => o.MapFrom(s =>
                s.RequestingDoctor == null ? "" : s.RequestingDoctor.FullName));

        // ── Service ───────────────────────────────────────────────────────────
        CreateMap<Service, ServiceResponseDto>();
        CreateMap<CreateServiceRequestDto, Service>();
        CreateMap<UpdateServiceRequestDto, Service>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));

        // ── Modification Requests ─────────────────────────────────────────────
        CreateMap<ServiceModificationRequest, ServiceModificationRequestResponseDto>()
            .ForMember(d => d.AdminName, o => o.MapFrom(s => s.Admin.FullName))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner == null ? "" : s.Owner.FullName));

        CreateMap<FAQModificationRequest, FAQModificationRequestResponseDto>()
            .ForMember(d => d.AdminName, o => o.MapFrom(s => s.Admin.FullName))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner == null ? "" : s.Owner.FullName));

        CreateMap<OfferDiscountModificationRequest, OfferDiscountModificationRequestResponseDto>()
            .ForMember(d => d.AdminName, o => o.MapFrom(s => s.Admin.FullName))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner == null ? "" : s.Owner.FullName));

        CreateMap<BranchModificationRequest, BranchModificationRequestResponseDto>()
            .ForMember(d => d.AdminName, o => o.MapFrom(s => s.Admin.FullName))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner == null ? "" : s.Owner.FullName));

        // ── Report ────────────────────────────────────────────────────────────
        CreateMap<Report, ReportResponseDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient == null ? "" : s.Patient.FullName))
            .ForMember(d => d.DoctorName,  o => o.MapFrom(s => s.Doctor  == null ? "" : s.Doctor.FullName));
            
        CreateMap<Report, AdminReportResponseDto>()
            .IncludeBase<Report, ReportResponseDto>();

        CreateMap<Report, DoctorReportResponseDto>()
            .IncludeBase<Report, AdminReportResponseDto>();
            
        // NOTE: ReportService.MapToResponse uses the above DTOs directly via AutoMapper but applies BR-57 role filtering manually by choosing which DTO to map to.
        CreateMap<CreateReportRequestDto, Report>();
        CreateMap<UpdateReportRequestDto, Report>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));

        CreateMap<ToothRecord, ToothRecordResponseDto>();

        // ── Prescription ──────────────────────────────────────────────────────
        CreateMap<Prescription, PrescriptionResponseDto>()
            .ForMember(d => d.PatientId, o => o.MapFrom(s => s.Report.PatientId))
            .ForMember(d => d.DoctorId,  o => o.MapFrom(s => s.Report.DoctorId));

        CreateMap<PrescriptionItem,             PrescriptionItemResponseDto>();
        CreateMap<CreatePrescriptionRequestDto, Prescription>();
        CreateMap<CreatePrescriptionItemRequestDto, PrescriptionItem>();

        // ── Notification ──────────────────────────────────────────────────────
        CreateMap<Notification, NotificationResponseDto>();

        // ── Branch ────────────────────────────────────────────────────────────
        CreateMap<Branch, BranchResponseDto>();
        CreateMap<CreateBranchRequestDto, Branch>();
        CreateMap<UpdateBranchRequestDto, Branch>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));

        // ── Offer Discount ────────────────────────────────────────────────────
        CreateMap<OfferDiscount, OfferDiscountResponseDto>()
            .ForMember(d => d.BranchName,  o => o.MapFrom(s => s.Branch  == null ? "" : s.Branch.Name))
            .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service == null ? "" : s.Service.Name));

        CreateMap<OfferDiscount, OfferStatusResponseDto>()
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch == null ? "" : s.Branch.Name));

        CreateMap<CreateOfferDiscountRequestDto, OfferDiscount>();
        CreateMap<UpdateOfferDiscountRequestDto, OfferDiscount>()
            .ForAllMembers(o => o.Condition((_, _, src) => src != null));

        // ── FAQ ───────────────────────────────────────────────────────────────
        CreateMap<FAQ, FAQResponseDto>();
        CreateMap<CreateFAQRequestDto, FAQ>();
        CreateMap<UpdateFAQRequestDto, FAQ>();

        // ── System Log ────────────────────────────────────────────────────────
        CreateMap<SystemLog, SystemLogResponseDto>();

        // ── Dental Chart ──────────────────────────────────────────────────────
        CreateMap<DentalChart, DentalChartResponseDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                s.Patient == null ? "" : s.Patient.FullName));
    }
}
