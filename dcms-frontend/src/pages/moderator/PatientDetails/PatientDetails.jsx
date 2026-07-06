import { useNavigate } from "react-router-dom";
import "./PatientDetails.css";

function PatientDetails() {
  const navigate = useNavigate();

  return (
    <div className="patient-details-page">
      <div className="details-header">
        <div>
          <h1>Patient Details</h1>
          <p>View patient profile and medical information.</p>
        </div>

        <button onClick={() => navigate("/moderator/patients")}>Back</button>
      </div>

      <div className="details-card">
        <section>
          <h2>Personal Information</h2>

          <p>
            <strong>Name:</strong> Ahmed Ali
          </p>
          <p>
            <strong>Phone:</strong> 01012345678
          </p>
          <p>
            <strong>Email:</strong> ahmed@gmail.com
          </p>
          <p>
            <strong>Gender:</strong> Male
          </p>
          <p>
            <strong>Age:</strong> 24
          </p>
        </section>

        <section>
          <h2>Medical Information</h2>

          <p>
            <strong>Blood Group:</strong> O+
          </p>
          <p>
            <strong>Allergies:</strong> None
          </p>
          <p>
            <strong>Chronic Diseases:</strong> None
          </p>
        </section>

        <section>
          <h2>Emergency Contact</h2>

          <p>
            <strong>Name:</strong> Mohamed Ali
          </p>
          <p>
            <strong>Phone:</strong> 01011111111
          </p>
        </section>

        <section>
          <h2>Notes</h2>

          <textarea rows="5" defaultValue="Patient is in good health." />
        </section>
      </div>
    </div>
  );
}

export default PatientDetails;
