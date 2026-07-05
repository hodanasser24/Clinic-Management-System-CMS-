import DashboardLayout from "../../layouts/DashboardLayout";
import "./PatientDashboard.css";

function PatientDashboard() {
  return (
    <DashboardLayout>
      <section className="patient-dashboard">
        <div className="welcome-card">
          <div>
            <p>Good Morning, Hoda 👋</p>
            <h2>Your dental care is managed in one place.</h2>
            <span>Track appointments, prescriptions, records and updates.</span>
          </div>

          <button>Book Appointment</button>
        </div>

        <div className="stats-grid">
          <div className="stat-card">
            <span>📅</span>
            <h3>3</h3>
            <p>Upcoming Appointments</p>
          </div>

          <div className="stat-card">
            <span>💊</span>
            <h3>5</h3>
            <p>Prescriptions</p>
          </div>

          <div className="stat-card">
            <span>📁</span>
            <h3>12</h3>
            <p>Medical Records</p>
          </div>

          <div className="stat-card">
            <span>🔔</span>
            <h3>2</h3>
            <p>Notifications</p>
          </div>
        </div>

        <div className="dashboard-grid">
          <div className="dashboard-panel">
            <h3>Upcoming Appointment</h3>

            <div className="appointment-card">
              <div>
                <strong>Dr. Ahmed Hassan</strong>
                <p>Dental Cleaning</p>
              </div>
              <span>Tomorrow · 10:30 AM</span>
            </div>
          </div>

          <div className="dashboard-panel">
            <h3>Quick Actions</h3>

            <div className="quick-actions">
              <button>Book Appointment</button>
              <button>View Records</button>
              <button>Prescriptions</button>
              <button>Update Profile</button>
            </div>
          </div>
        </div>
      </section>
    </DashboardLayout>
  );
}

export default PatientDashboard;
