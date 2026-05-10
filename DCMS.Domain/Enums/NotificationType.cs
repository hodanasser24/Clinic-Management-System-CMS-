// File: DCMS.Domain/Enums/NotificationType.cs
namespace DCMS.Domain.Enums;

public enum NotificationType
{
    // Appointment lifecycle
    AppointmentBooked,
    AppointmentConfirmed,
    AppointmentRejected,
    AppointmentCancelled,
    AppointmentCompleted,
    AppointmentReminder,
    AppointmentRescheduled,
    AppointmentMarkedUrgent,
    AppointmentUrgentUnmarked,
    AttendanceMarked,
    FollowUpScheduled,

    // Schedule Change Requests
    ScheduleChangeRequestSubmitted,
    ScheduleChangeRequestApproved,
    ScheduleChangeRequestRejected,
    ScheduleChangeApprovedByDoctor,
    ScheduleChangeApprovedByOwner,
    ScheduleChangeExpired,

    // Modification requests
    ServiceModificationRequestSubmitted,
    ServiceModificationRequestApproved,
    ServiceModificationRequestRejected,
    FAQModificationRequestSubmitted,
    FAQModificationRequestApproved,
    FAQModificationRequestRejected,
    OfferModificationRequestSubmitted,
    OfferModificationRequestApproved,
    OfferModificationRequestRejected,
    BranchModificationRequestSubmitted,
    BranchModificationRequestApproved,
    BranchModificationRequestRejected,

    // Offer
    OfferActivated,
    OfferDeactivated,
    OfferExpired,

    // Account
    AccountCreated,
    AccountDeactivated,
    PasswordChanged,
    FirstLoginPrompt,

    // Reports & Prescriptions
    ReportCreated,
    PrescriptionCreated,

    // General
    ContactMessageReceived,
    ContactMessageReplied,
    NewContactMessage,
    SystemAlert
}
