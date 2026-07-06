import { useNavigate } from "react-router-dom";
import "./PatientDetails.css";

function PatientDetails() {
  const navigate = useNavigate();

  return (
    <div className="doctor-patient-details-page">
      <div className="patient-header">
        <div>
          <h1>Patient Details</h1>
          <p>Complete medical information for this patient.</p>
        </div>

        <button onClick={() => navigate("/doctor/patients")}>Back</button>
      </div>

      <div className="patient-grid">
        <div className="patient-card">
          <h2>Personal Information</h2>

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
            <strong>Email:</strong> ahmed@gmail.com
          </p>
          <p>
            <strong>Blood Type:</strong> O+
          </p>
        </div>

        <div className="patient-card">
          <h2>Medical History</h2>

          <p>
            <strong>Allergies:</strong> None
          </p>
          <p>
            <strong>Chronic Diseases:</strong> None
          </p>
          <p>
            <strong>Previous Visits:</strong> 6
          </p>
          <p>
            <strong>Last Visit:</strong> 05 Jul 2026
          </p>
          <p>
            <strong>Current Medication:</strong> None
          </p>
        </div>
      </div>

      <div className="patient-card">
        <h2>Treatment History</h2>

        <ul>
          <li>✔ Teeth Cleaning - Jan 2026</li>
          <li>✔ Dental Filling - Mar 2026</li>
          <li>✔ Routine Checkup - Jul 2026</li>
        </ul>
      </div>

      <div className="patient-card">
        <h2>Doctor Notes</h2>

        <textarea rows="6" placeholder="Write notes..."></textarea>
      </div>

      <div className="patient-actions">
        <button onClick={() => navigate("/doctor/medical-records")}>
          Medical Record
        </button>

        <button onClick={() => navigate("/doctor/prescriptions")}>
          Prescription
        </button>

        <button className="success">Save Notes</button>
      </div>
    </div>
  );
}

export default PatientDetails;
