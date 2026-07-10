import { useNavigate } from "react-router-dom";
import "./AppointmentDetails.css";

function AppointmentDetails() {
  const navigate = useNavigate();

  return (
    <div className="patient-details-page">
      <div className="details-header">
        <div>
          <h1>Appointment Details</h1>
          <p>View complete information about your appointment.</p>
        </div>

        <button onClick={() => navigate("/patient/appointments")}>Back</button>
      </div>

      <div className="details-grid">
        <div className="details-card">
          <h2>Doctor Information</h2>

          <p>
            <strong>Doctor:</strong> Dr. Sara Ahmed
          </p>
          <p>
            <strong>Department:</strong> Orthodontics
          </p>
          <p>
            <strong>Clinic:</strong> Main Clinic
          </p>
          <p>
            <strong>Phone:</strong> 01012345678
          </p>
        </div>

        <div className="details-card">
          <h2>Appointment Information</h2>

          <p>
            <strong>Date:</strong> 06 Jul 2026
          </p>
          <p>
            <strong>Time:</strong> 10:30 AM
          </p>
          <p>
            <strong>Status:</strong>{" "}
            <span className="confirmed">Confirmed</span>
          </p>
          <p>
            <strong>Room:</strong> Room 4
          </p>
        </div>
      </div>

      <div className="details-card">
        <h2>Appointment Notes</h2>

        <textarea
          rows="6"
          placeholder="Doctor notes will appear here..."
          readOnly
        />
      </div>

      <div className="details-actions">
        <button onClick={() => navigate("/patient/medical-records")}>
          Medical Record
        </button>

        <button onClick={() => navigate("/patient/prescriptions")}>
          Prescription
        </button>

        <button className="danger" onClick={() => alert("Appointment has been cancelled.")}>Cancel Appointment</button>
      </div>
    </div>
  );
}

export default AppointmentDetails;
