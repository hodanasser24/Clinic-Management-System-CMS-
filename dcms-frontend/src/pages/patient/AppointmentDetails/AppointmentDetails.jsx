import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "./AppointmentDetails.css";

import { getAppointmentById, cancelAppointment, rescheduleAppointment } from "../../../services/appointmentServices";
import { getAvailableSlots } from "../../../services/publicServices";
import Loading from "../../../components/common/Loading/Loading";
import EmptyState from "../../../components/common/EmptyState/EmptyState";
import Input from "../../../components/common/Input";

function AppointmentDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [appointment, setAppointment] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Cancellation state
  const [showCancelPrompt, setShowCancelPrompt] = useState(false);
  const [cancelReason, setCancelReason] = useState("");
  const [cancelling, setCancelling] = useState(false);

  // Reschedule state
  const [showReschedulePrompt, setShowReschedulePrompt] = useState(false);
  const [newDate, setNewDate] = useState("");
  const [availableSlots, setAvailableSlots] = useState([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState("");
  const [rescheduling, setRescheduling] = useState(false);

  const fetchAppointment = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAppointmentById(id);
      setAppointment(data);
    } catch (err) {
      setError("Failed to load appointment details. Please try again later.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchAppointment();
  }, [fetchAppointment]);

  const handleCancel = async () => {
    if (!cancelReason.trim()) {
      alert("Please provide a reason for cancellation.");
      return;
    }

    try {
      setCancelling(true);
      await cancelAppointment(id, cancelReason);
      setShowCancelPrompt(false);
      // Refresh the data to show updated status
      fetchAppointment();
    } catch (err) {
      alert(err.message || "Failed to cancel appointment.");
    } finally {
      setCancelling(false);
    }
  };

  const fetchSlots = useCallback(async () => {
    if (!newDate || !appointment) {
      setAvailableSlots([]);
      setSelectedSlot("");
      return;
    }

    try {
      setLoadingSlots(true);
      setSelectedSlot(""); // reset selected slot when changing date
      const slots = await getAvailableSlots(appointment.doctorId, appointment.branchId, newDate);
      setAvailableSlots(slots || []);
    } catch (err) {
      console.error("Failed to load time slots", err);
      setAvailableSlots([]);
    } finally {
      setLoadingSlots(false);
    }
  }, [newDate, appointment]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchSlots();
  }, [fetchSlots]);

  const handleReschedule = async () => {
    if (!newDate || !selectedSlot) {
      alert("Please select a new date and time slot.");
      return;
    }

    try {
      setRescheduling(true);
      await rescheduleAppointment(id, {
        newDate: newDate,
        newStartTime: selectedSlot
      });
      setShowReschedulePrompt(false);
      setNewDate("");
      setSelectedSlot("");
      // Refresh to get updated details
      fetchAppointment();
    } catch (err) {
      alert(err.message || "Failed to reschedule appointment.");
    } finally {
      setRescheduling(false);
    }
  };

  if (loading) return <div className="appointment-details-page"><Loading /></div>;
  if (error) return <div className="appointment-details-page"><EmptyState title="Error" message={error} /></div>;
  if (!appointment) return <div className="appointment-details-page"><EmptyState title="Not Found" message="Appointment not found." /></div>;

  const isCancellable = appointment.status === "Pending" || appointment.status === "Confirmed" || appointment.status === "Rescheduled";

  return (
    <div className="appointment-details-page">
      <button className="back-btn" onClick={() => navigate("/patient/appointments")} style={{ marginBottom: "20px", background: "none", border: "none", color: "#60a5fa", cursor: "pointer", fontSize: "16px" }}>
        &larr; Back to Appointments
      </button>

      <h1>Appointment Details</h1>

      <div className="details-card">
        <section>
          <h2>Doctor Information</h2>
          <p>
            <strong>Doctor:</strong> {appointment.doctorName}
          </p>
          <p>
            <strong>Branch:</strong> {appointment.branchName}
          </p>
          <p>
            <strong>Service:</strong> {appointment.serviceName}
          </p>
        </section>

        <section>
          <h2>Appointment Information</h2>
          <p>
            <strong>ID:</strong> #{appointment.id}
          </p>
          <p>
            <strong>Date:</strong> {appointment.date}
          </p>
          <p>
            <strong>Time:</strong> {appointment.startTime} - {appointment.endTime}
          </p>
          <p>
            <strong>Status:</strong> <span className={`status-badge ${appointment.status?.toLowerCase()}`}>{appointment.status}</span>
          </p>
          <p>
            <strong>Attendance:</strong> {appointment.attendanceStatus}
          </p>
          {appointment.notes && (
            <p>
              <strong>Notes:</strong> {appointment.notes}
            </p>
          )}
        </section>

        {showCancelPrompt ? (
          <section>
            <h2>Cancel Appointment</h2>
            <textarea
              rows="4"
              placeholder="Please explain why you are cancelling..."
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              disabled={cancelling}
            ></textarea>
            <div className="details-actions" style={{ marginTop: "16px" }}>
              <button className="danger" onClick={handleCancel} disabled={cancelling}>
                {cancelling ? "Cancelling..." : "Confirm Cancellation"}
              </button>
              <button onClick={() => setShowCancelPrompt(false)} disabled={cancelling} style={{ background: "transparent", border: "1px solid #94a3b8" }}>
                Keep Appointment
              </button>
            </div>
          </section>
        ) : showReschedulePrompt ? (
          <section>
            <h2>Reschedule Appointment</h2>
            <div className="reschedule-form" style={{ marginTop: "16px" }}>
              <Input
                label="New Date *"
                type="date"
                value={newDate}
                onChange={(e) => setNewDate(e.target.value)}
                min={new Date().toISOString().split("T")[0]}
                disabled={rescheduling}
              />

              {newDate && (
                <div className="time-slots-container" style={{ marginTop: "24px" }}>
                  <h3 style={{ fontSize: "16px", marginBottom: "12px" }}>Available Time Slots *</h3>
                  {loadingSlots ? (
                    <p style={{ color: "#94a3b8", fontSize: "14px" }}>Loading slots...</p>
                  ) : availableSlots.length === 0 ? (
                    <p style={{ color: "#ef4444", fontSize: "14px" }}>
                      No available slots for the selected doctor on this date. Please try another date.
                    </p>
                  ) : (
                    <div className="time-slots-grid">
                      {availableSlots.map((slot, index) => (
                        <button
                          key={index}
                          className={`time-slot-btn ${selectedSlot === slot.startTime ? "selected" : ""}`}
                          onClick={() => setSelectedSlot(slot.startTime)}
                          disabled={rescheduling}
                          type="button"
                        >
                          {slot.startTime.substring(0, 5)}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
            <div className="details-actions" style={{ marginTop: "24px" }}>
              <button className="submit-booking-btn" onClick={handleReschedule} disabled={rescheduling || !newDate || !selectedSlot}>
                {rescheduling ? "Rescheduling..." : "Confirm Reschedule"}
              </button>
              <button onClick={() => { setShowReschedulePrompt(false); setNewDate(""); setSelectedSlot(""); }} disabled={rescheduling} style={{ background: "transparent", border: "1px solid #94a3b8" }}>
                Cancel
              </button>
            </div>
          </section>
        ) : (
          isCancellable && (
            <div className="details-actions">
              <button onClick={() => setShowReschedulePrompt(true)} className="submit-booking-btn">
                Reschedule Appointment
              </button>
              <button className="danger" onClick={() => setShowCancelPrompt(true)}>
                Cancel Appointment
              </button>
            </div>
          )
        )}
      </div>
    </div>
  );
}

export default AppointmentDetails;
