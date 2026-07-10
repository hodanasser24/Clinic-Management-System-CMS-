import { useNavigate } from "react-router-dom";
import "./MedicalRecords.css";
import DentalChart from "../../../components/common/DentalChart/DentalChart";

function MedicalRecords() {
  const navigate = useNavigate();

  return (
    <div className="medical-page">
      <div className="medical-header">
        <div>
          <h1>Medical Records</h1>
          <p>Manage patient's diagnosis and treatment history.</p>
        </div>

        <button onClick={() => navigate("/doctor/patients")}>Back</button>
      </div>

      <div className="medical-card">
        <h2>Patient Information</h2>
        <DentalChart />

        <div className="medical-grid">
          <p>
            <strong>Name:</strong> Ahmed Ali
          </p>
          <p>
            <strong>Age:</strong> 24
          </p>
          <p>
            <strong>Blood Type:</strong> O+
          </p>
          <p>
            <strong>Phone:</strong> 01012345678
          </p>
        </div>
      </div>

      <div className="medical-card">
        <h2>Diagnosis</h2>

        <textarea rows="5" placeholder="Write diagnosis..." />
      </div>

      <div className="medical-card">
        <h2>Treatment Plan</h2>

        <textarea rows="5" placeholder="Write treatment plan..." />
      </div>

      <div className="medical-card">
        <h2>Medications</h2>

        <textarea rows="4" placeholder="Current medications..." />
      </div>

      <div className="medical-card">
        <h2>Doctor Notes</h2>

        <textarea rows="6" placeholder="Additional notes..." />
      </div>

      <div className="medical-actions">
        <button onClick={() => navigate("/doctor/prescriptions")}>
          Create Prescription
        </button>

        <button className="success" onClick={() => alert("Medical record saved successfully.")}>Save Record</button>
      </div>
    </div>
  );
}

export default MedicalRecords;
