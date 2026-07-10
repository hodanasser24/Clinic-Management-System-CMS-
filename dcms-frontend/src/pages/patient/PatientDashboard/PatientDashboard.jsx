import { useNavigate } from "react-router-dom";
import "./PatientDashboard.css";

function PatientDashboard() {
  const navigate = useNavigate();

  return (
    <div className="patient-dashboard-page">
      <div className="patient-header">
        <div>
          <h1>Welcome, Ahmed Ali 👋</h1>
          <p>Manage your appointments and medical records easily.</p>
        </div>

        <button onClick={() => navigate("/patient/profile")}>My Profile</button>
      </div>

      <div className="patient-stats">
        <div className="patient-card">
          <h3>Upcoming Appointments</h3>
          <span>2</span>
        </div>

        <div className="patient-card">
          <h3>Completed Visits</h3>
          <span>12</span>
        </div>

        <div className="patient-card">
          <h3>Prescriptions</h3>
          <span>4</span>
        </div>

        <div className="patient-card">
          <h3>Notifications</h3>
          <span>3</span>
        </div>
      </div>

      <div className="patient-grid">
        <div className="patient-section">
          <h2>Next Appointment</h2>

          <div className="appointment-box">
            <h3>Dr. Sara Ahmed</h3>

            <p>Orthodontics</p>

            <p>Monday, 06 Jul 2026</p>

            <p>10:30 AM</p>

            <span className="confirmed">Confirmed</span>

            <button onClick={() => navigate("/patient/appointments/1")}>
              View Details
            </button>
          </div>
        </div>

        <div className="patient-section">
          <h2>Medical Summary</h2>

          <p>
            <strong>Blood Type:</strong> O+
          </p>

          <p>
            <strong>Allergies:</strong> None
          </p>

          <p>
            <strong>Chronic Diseases:</strong> None
          </p>

          <p>
            <strong>Last Visit:</strong> 01 Jul 2026
          </p>
        </div>
      </div>

      <div className="patient-section">
        <h2>Quick Actions</h2>

        <div className="quick-buttons">
          <button onClick={() => navigate("/patient/appointments")}>
            My Appointments
          </button>

          <button onClick={() => navigate("/patient/medical-records")}>
            Medical Records
          </button>

          <button onClick={() => navigate("/patient/prescriptions")}>
            Prescriptions
          </button>

          <button onClick={() => navigate("/patient/profile")}>Profile</button>
        </div>
      </div>
    </div>
  );
}

export default PatientDashboard;
