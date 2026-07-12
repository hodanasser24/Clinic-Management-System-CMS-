import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getAppointmentById, cancelAppointment, markAttendance } from "../../../services/appointmentServices";
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

  const initiateCancel = () => {
    setCancelModalOpen(true);
  };

  const handleCancel = async (reason) => {
    try {
      await cancelAppointment(id, reason);
      setCancelModalOpen(false);
      loadData();
    } catch (err) {
      alert("Failed to cancel: " + err.message);
    }
  };

  const handleMarkAttendance = async () => {
    if (!window.confirm("Mark patient as present (Completed)?")) return;
    try {
      await markAttendance(id, { attendanceStatus: 1 }); // 1 = Present / Completed
      loadData();
    } catch (err) {
      alert("Failed to mark attendance: " + err.message);
    }
  };

  if (loading) return <div className="doctor-details-page"><p>Loading details...</p></div>;
  if (error) return <div className="doctor-details-page"><p className="error">{error}</p></div>;
  if (!appointment) return <div className="doctor-details-page"><p>Appointment not found.</p></div>;

  return (
    <div className="doctor-details-page">
      <div className="doctor-details-header">
        <div>
          <h1>Appointment Details</h1>
          <p>Review patient visit, diagnosis and treatment plan.</p>
        </div>

        <button onClick={() => navigate("/doctor/appointments")}>Back</button>
      </div>

      <div className="details-grid">
        <div className="details-card">
          <h2>Patient Information</h2>
          <p><strong>Name:</strong> {appointment.patientName}</p>
          <p><strong>Patient ID:</strong> {appointment.patientId}</p>
        </div>

        <div className="details-card">
          <h2>Appointment Information</h2>
          <p><strong>Date:</strong> {appointment.date}</p>
          <p><strong>Time:</strong> {appointment.startTime}</p>
          <p><strong>Service:</strong> {appointment.serviceName}</p>
          <p><strong>Branch:</strong> {appointment.branchName}</p>
          <p>
            <strong>Status:</strong>{" "}
            <span className={`status-badge ${appointment.status?.toLowerCase()}`}>
              {appointment.status}
            </span>
          </p>
          <p><strong>Attendance:</strong> {appointment.attendanceStatus}</p>
        </div>
      </div>

      <div className="details-card">
        <h2>Medical History & Notes</h2>
        <p>
          <strong>Notes:</strong> {appointment.notes || "None provided"}
        </p>
      </div>

      <div className="details-card">
        <h2>Diagnosis</h2>
        <textarea rows="4" placeholder="Write diagnosis here... (Mocked for now)" />
      </div>

      <div className="details-card">
        <h2>Treatment Plan</h2>
        <textarea rows="4" placeholder="Write treatment plan here... (Mocked for now)" />
      </div>

      <div className="doctor-actions">
        {appointment.status === "Confirmed" && (
          <button onClick={handleMarkAttendance}>Complete Appointment</button>
        )}
        <button onClick={() => navigate("/doctor/prescriptions")}>
          + Add Prescription
        </button>
        {["Pending", "Confirmed"].includes(appointment.status) && (
          <button className="danger" onClick={initiateCancel}>Cancel Appointment</button>
        )}
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
