import { useNavigate } from "react-router-dom";
import "./EditAppointment.css";

function EditAppointment() {
  const navigate = useNavigate();

  return (
    <div className="appointment-form-page">
      <div className="form-header">
        <div>
          <h1>Edit Appointment</h1>
          <p>Update appointment information.</p>
        </div>

        <button onClick={() => navigate("/moderator/appointments")}>
          Back
        </button>
      </div>

      <form className="appointment-form">
        <div className="form-grid">
          <div className="form-group">
            <label>Patient Name</label>
            <input defaultValue="Ahmed Ali" />
          </div>

          <div className="form-group">
            <label>Phone Number</label>
            <input defaultValue="01012345678" />
          </div>

          <div className="form-group">
            <label>Doctor</label>
            <select defaultValue="Dr. Sara">
              <option>Dr. Sara</option>
              <option>Dr. Omar</option>
              <option>Dr. Ahmed</option>
            </select>
          </div>

          <div className="form-group">
            <label>Service</label>
            <select defaultValue="Consultation">
              <option>Consultation</option>
              <option>Teeth Cleaning</option>
              <option>Root Canal</option>
            </select>
          </div>

          <div className="form-group">
            <label>Date</label>
            <input type="date" defaultValue="2026-07-10" />
          </div>

          <div className="form-group">
            <label>Time</label>
            <input type="time" defaultValue="10:30" />
          </div>

          <div className="form-group">
            <label>Status</label>
            <select defaultValue="Pending">
              <option>Pending</option>
              <option>Confirmed</option>
              <option>Completed</option>
            </select>
          </div>

          <div className="form-group">
            <label>Branch</label>
            <select defaultValue="Main Branch">
              <option>Main Branch</option>
              <option>Nasr City Branch</option>
            </select>
          </div>
        </div>

        <div className="form-group full">
          <label>Notes</label>
          <textarea
            rows="5"
            defaultValue="Patient needs follow-up after consultation."
          />
        </div>

        <div className="form-actions">
          <button
            type="button"
            onClick={() => navigate("/moderator/appointments")}
          >
            Cancel
          </button>

          <button className="primary">Update Appointment</button>
        </div>
      </form>
    </div>
  );
}

export default EditAppointment;
