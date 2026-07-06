import { useNavigate } from "react-router-dom";
import "./Prescriptions.css";

function Prescriptions() {
  const navigate = useNavigate();

  return (
    <div className="prescription-page">
      <div className="prescription-header">
        <div>
          <h1>Create Prescription</h1>
          <p>Generate a prescription for the current patient.</p>
        </div>

        <button onClick={() => navigate("/doctor/appointments")}>Back</button>
      </div>

      <div className="prescription-card">
        <h2>Patient</h2>

        <p>
          <strong>Name:</strong> Ahmed Ali
        </p>
        <p>
          <strong>Age:</strong> 24
        </p>
        <p>
          <strong>Diagnosis:</strong> Gingivitis
        </p>
      </div>

      <div className="prescription-card">
        <h2>Prescription</h2>

        <textarea
          rows="8"
          placeholder="Write medicines, dosage and instructions..."
        />
      </div>

      <div className="prescription-card">
        <h2>Additional Instructions</h2>

        <textarea rows="4" placeholder="Extra recommendations..." />
      </div>

      <div className="prescription-actions">
        <button>Print</button>

        <button>Download PDF</button>

        <button className="success">Save Prescription</button>
      </div>
    </div>
  );
}

export default Prescriptions;
