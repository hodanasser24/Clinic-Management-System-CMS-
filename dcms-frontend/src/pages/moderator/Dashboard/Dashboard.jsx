import { useNavigate } from "react-router-dom";
import "./Dashboard.css";

function Dashboard() {
  const navigate = useNavigate();

  const recentAppointments = [
    {
      id: 1,
      patient: "Ahmed Ali",
      doctor: "Dr. Sara",
      time: "10:00 AM",
      status: "Pending",
    },
    {
      id: 2,
      patient: "Mona Hassan",
      doctor: "Dr. Omar",
      time: "11:30 AM",
      status: "Completed",
    },
  ];

  const recentPatients = [
    { id: 1, name: "Ahmed Ali", phone: "01012345678", status: "Active" },
    { id: 2, name: "Mona Hassan", phone: "01098765432", status: "Active" },
  ];

  const activities = [
    "New appointment created by Ahmed Ali",
    "Mona Hassan appointment marked as completed",
    "New patient profile added",
  ];

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <div>
          <h1>Moderator Dashboard</h1>
          <p>Welcome back! Here's today's clinic overview.</p>
        </div>

        <button onClick={() => navigate("/moderator/appointments/add")}>
          + New Appointment
        </button>
      </div>

      <div className="stats-cards">
        <div className="stat-card">
          <h3>Today's Appointments</h3>
          <span>32</span>
        </div>
        <div className="stat-card">
          <h3>Patients Today</h3>
          <span>18</span>
        </div>
        <div className="stat-card">
          <h3>Completed</h3>
          <span>24</span>
        </div>
        <div className="stat-card">
          <h3>Cancelled</h3>
          <span>3</span>
        </div>
      </div>

      <div className="quick-actions">
        <button onClick={() => navigate("/moderator/appointments")}>
          Appointments
        </button>
        <button onClick={() => navigate("/moderator/patients")}>
          Patients
        </button>
        <button onClick={() => navigate("/moderator/reports")}>Reports</button>
      </div>

      <div className="dashboard-grid">
        <div className="dashboard-section">
          <h2>Recent Appointments</h2>
          {recentAppointments.map((item) => (
            <div className="mini-row" key={item.id}>
              <div>
                <strong>{item.patient}</strong>
                <p>
                  {item.doctor} • {item.time}
                </p>
              </div>
              <span>{item.status}</span>
            </div>
          ))}
        </div>

        <div className="dashboard-section">
          <h2>Recent Patients</h2>
          {recentPatients.map((item) => (
            <div className="mini-row" key={item.id}>
              <div>
                <strong>{item.name}</strong>
                <p>{item.phone}</p>
              </div>
              <span>{item.status}</span>
            </div>
          ))}
        </div>
      </div>

      <div className="dashboard-section">
        <h2>Today’s Activity</h2>
        {activities.map((activity, index) => (
          <div className="activity-item" key={index}>
            • {activity}
          </div>
        ))}
      </div>
    </div>
  );
}

export default Dashboard;
