import { useNavigate } from "react-router-dom";
import "./AppointmentDetails.css";

function AppointmentDetails() {
  const navigate = useNavigate();

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
        </section>

        <section>
          <h2>Doctor Information</h2>

          <p>
            <strong>Doctor:</strong> Dr. Sara
          </p>

          <p>
            <strong>Department:</strong> Orthodontics
          </p>
        </section>

        <section>
          <h2>Appointment Information</h2>

          <p>
            <strong>Appointment ID:</strong> #1005
          </p>

          <p>
            <strong>Date:</strong> 05 Jul 2026
          </p>

          <p>
            <strong>Time:</strong> 10:00 AM
          </p>

          <p>
            <strong>Status:</strong>{" "}
            <span className="status-badge pending">Pending</span>
          </p>

          <p>
            <strong>Reason:</strong> Teeth Cleaning
          </p>
        </section>

        <section>
          <h2>Notes</h2>

          <textarea
            rows="5"
            defaultValue="Patient requested a regular teeth cleaning appointment."
          />
        </section>

        <div className="details-actions">
          <button>Confirm</button>

          <button>Reschedule</button>

          <button onClick={() => navigate("/moderator/appointments/edit/1")}>
            Edit
          </button>

          <button className="danger">Cancel</button>
        </div>
      </div>
    </div>
  );
}

export default AppointmentDetails;
