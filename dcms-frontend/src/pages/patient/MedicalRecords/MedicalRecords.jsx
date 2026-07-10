import { useNavigate } from "react-router-dom";
import "./MedicalRecords.css";
import DentalChart from "../../../components/common/DentalChart/DentalChart";

function MedicalRecords() {
  const navigate = useNavigate();

  return (
    <div className="patient-medical-page">
      <div className="medical-header">
        <div>
          <h1>Medical Records</h1>
          <p>Your complete medical history.</p>
        </div>

        <button onClick={() => navigate("/patient/dashboard")}>
          Dashboard
        </button>
      </div>

      <div className="medical-card">
        <h2>Personal Information</h2>

        <div className="medical-grid">
          <p>
            <strong>Name:</strong> Ahmed Ali
          </p>
          <p>
            <strong>Blood Type:</strong> O+
          </p>
          <p>
            <strong>Allergies:</strong> None
          </p>
          <p>
            <strong>Chronic Diseases:</strong> None
          </p>
        </div>
      </div>

      <div className="medical-card">
        <h2>Dental Chart</h2>
        <DentalChart readOnly={true} />
      </div>

      <div className="medical-card">
        <h2>Medical History</h2>

        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Doctor</th>
              <th>Diagnosis</th>
              <th>Treatment</th>
            </tr>
          </thead>

          <tbody>
            <tr>
              <td>01 Jul 2026</td>
              <td>Dr. Sara</td>
              <td>Teeth Cleaning</td>
              <td>Completed</td>
            </tr>

            <tr>
              <td>18 Jun 2026</td>
              <td>Dr. Ahmed</td>
              <td>Dental Filling</td>
              <td>Completed</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default MedicalRecords;
