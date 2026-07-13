import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getAppointmentById, confirmAppointment, rejectAppointment, cancelAppointment } from "../../../services/appointmentServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import CancelModal from "../../../components/ui/CancelModal/CancelModal";
import "./AppointmentDetails.css";

function AppointmentDetails() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [appointment, setAppointment] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [cancelModalOpen, setCancelModalOpen] = useState(false);

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");
      const data = await getAppointmentById(id);
      setAppointment(data);
    } catch (err) {
      setError(err.message || "Failed to load appointment details.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [id]);

  const handleConfirm = async () => {
    if (!window.confirm("Confirm this appointment?")) return;
    try {
      await confirmAppointment(id);
      loadData();
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Unknown error";
      alert("Failed to confirm: " + errorMsg);
    }
  };

  const handleReject = async () => {
    if (!window.confirm("Reject this appointment?")) return;
    try {
      await rejectAppointment(id);
      loadData();
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Unknown error";
      alert("Failed to reject: " + errorMsg);
    }
  };

  const initiateCancel = () => {
    setCancelModalOpen(true);
  };

  const handleCancel = async (reason) => {
    try {
      await cancelAppointment(id, reason);
      setCancelModalOpen(false);
      loadData();
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Unknown error";
      alert("Failed to cancel: " + errorMsg);
    }
  };

  if (loading) return <div className="appointment-details-page"><p>Loading details...</p></div>;
  if (error) return <div className="appointment-details-page"><p className="error">{error}</p></div>;
  if (!appointment) return <div className="appointment-details-page"><p>Appointment not found.</p></div>;

  return (
    <div className="appointment-details-page">
      <div className="details-header">
        <div>
          <h1>Appointment Details</h1>
          <p>View complete appointment information.</p>
        </div>

        <button onClick={() => navigate("/moderator/appointments")}>
          Back
        </button>
      </div>

      <div className="details-card">
        <section>
          <h2>Patient Information</h2>
          <p><strong>Name:</strong> {appointment.patientName}</p>
          <p><strong>Patient ID:</strong> {appointment.patientId}</p>
        </section>

        <section>
          <h2>Doctor Information</h2>
          <p><strong>Doctor:</strong> Dr. {appointment.doctorName}</p>
          <p><strong>Service:</strong> {appointment.serviceName}</p>
          <p><strong>Branch:</strong> {appointment.branchName}</p>
        </section>

        <section>
          <h2>Appointment Information</h2>
          <p><strong>Appointment ID:</strong> #{appointment.id}</p>
          <p><strong>Date:</strong> {appointment.date}</p>
          <p><strong>Time:</strong> {formatTo12Hour(appointment.startTime)}</p>
          <p>
            <strong>Status:</strong>{" "}
            <span className={`status-badge ${appointment.status?.toLowerCase()}`}>
              {appointment.status}
            </span>
          </p>
          <p><strong>Attendance:</strong> {appointment.attendanceStatus}</p>
        </section>

        <section>
          <h2>Notes</h2>
          <textarea
            rows="5"
            readOnly
            value={appointment.notes || "No notes provided."}
          />
        </section>

        <div className="details-actions">
          {appointment.status === "Pending" && (
            <>
              <button onClick={handleConfirm}>Confirm</button>
              <button className="danger" onClick={handleReject}>Reject</button>
            </>
          )}

          {/* Navigation to edit */}
          <button onClick={() => navigate(`/moderator/appointments/edit/${id}`)}>
            Edit
          </button>
        </div>
      </div>

      <CancelModal
        isOpen={cancelModalOpen}
        onClose={() => setCancelModalOpen(false)}
        onConfirm={handleCancel}
      />
    </div>
  );
}

export default AppointmentDetails;
