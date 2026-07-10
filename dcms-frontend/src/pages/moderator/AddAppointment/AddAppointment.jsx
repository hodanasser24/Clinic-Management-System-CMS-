import { useNavigate } from "react-router-dom";
import "./AddAppointment.css";

function AddAppointment() {
  const navigate = useNavigate();

  return (
    <div className="appointment-form-page">
      <div className="form-header">
        <div>
          <h1>Add Appointment</h1>
          <p>Create a new clinic appointment.</p>
        </div>

        <button onClick={() => navigate("/moderator/appointments")}>
          Back
        </button>
      </div>

      <form className="appointment-form">
        <div className="form-grid">
          <div className="form-group">
            <label>Patient Name</label>
            <input placeholder="Enter patient name" />
          </div>

          <div className="form-group">
            <label>Phone Number</label>
            <input placeholder="Enter phone number" />
          </div>

          <div className="form-group">
            <label>Doctor</label>
            <select>
              <option>Dr. Sara</option>
              <option>Dr. Omar</option>
              <option>Dr. Ahmed</option>
            </select>
          </div>

          <div className="form-group">
            <label>Service</label>
            <select>
              <option>Teeth Cleaning</option>
              <option>Orthodontics</option>
              <option>Root Canal</option>
              <option>Consultation</option>
            </select>
          </div>

          <div className="form-group">
            <label>Date</label>
            <input type="date" />
          </div>

          <div className="form-group">
            <label>Time</label>
            <input type="time" />
          </div>

          <div className="form-group">
            <label>Status</label>
            <select>
              <option>Pending</option>
              <option>Confirmed</option>
              <option>Completed</option>
              <option>Cancelled</option>
            </select>
          </div>

          <div className="form-group">
            <label>Branch</label>
            <select>
              <option>Main Branch</option>
              <option>Nasr City Branch</option>
              <option>Maadi Branch</option>
            </select>
          </div>
        </div>

        <div className="form-group full">
          <label>Notes</label>
          <textarea rows="5" placeholder="Write appointment notes..." />
        </div>

        <div className="form-actions">
          <button
            type="button"
            onClick={() => navigate("/moderator/appointments")}
          >
            Cancel
          </button>

          <button type="submit" className="primary">
            Save Appointment
          </button>
        </div>
      </form>
    </div>
  );
}

export default AddAppointment;
