import { useNavigate } from "react-router-dom";
import "./AppointmentDetails.css";

function AppointmentDetails() {
  const navigate = useNavigate();

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
          <p>
            <strong>Name:</strong> Ahmed Ali
          </p>
          <p>
            <strong>Age:</strong> 24
          </p>
          <p>
            <strong>Gender:</strong> Male
          </p>
          <p>
            <strong>Phone:</strong> 01012345678
          </p>
          <p>
            <strong>Blood Type:</strong> O+
          </p>
          <p>
            <strong>Allergies:</strong> None
          </p>
        </div>

        <div className="details-card">
          <h2>Appointment Information</h2>
          <p>
            <strong>Date:</strong> 05 Jul 2026
          </p>
          <p>
            <strong>Time:</strong> 10:00 AM
          </p>
          <p>
            <strong>Service:</strong> Teeth Cleaning
          </p>
          <p>
            <strong>Branch:</strong> Main Branch
          </p>
          <p>
            <strong>Status:</strong>{" "}
            <span className="status-badge pending">Pending</span>
          </p>
        </div>
      </div>

      <div className="details-card">
        <h2>Medical History</h2>
        <p>
          <strong>Previous Visits:</strong> 3 visits
        </p>
        <p>
          <strong>Chronic Diseases:</strong> None
        </p>
        <p>
          <strong>Current Medications:</strong> None
        </p>
      </div>

      <div className="details-card">
        <h2>Diagnosis</h2>
        <textarea rows="4" placeholder="Write diagnosis here..." />
      </div>

      <div className="details-card">
        <h2>Treatment Plan</h2>
        <textarea rows="4" placeholder="Write treatment plan here..." />
      </div>

      <div className="doctor-actions">
        <button onClick={() => alert("Appointment marked as completed.")}>Complete Appointment</button>
        <button onClick={() => navigate("/doctor/prescriptions")}>
          + Add Prescription
        </button>
        <button className="danger" onClick={() => alert("Appointment has been cancelled.")}>Cancel Appointment</button>
      </div>
    </div>
  );
}

export default AppointmentDetails;
