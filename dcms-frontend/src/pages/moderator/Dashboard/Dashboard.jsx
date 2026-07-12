import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import "./Dashboard.css";

function Dashboard() {
  const navigate = useNavigate();
  const [stats, setStats] = useState(null);
  const [recentAppointments, setRecentAppointments] = useState([]);
  const [recentPatients, setRecentPatients] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [statsRes, appointmentsRes, patientsRes] = await Promise.all([
          apiClient.get("/api/Dashboard/summary"),
          apiClient.get("/api/Appointment?page=1&pageSize=5"),
          apiClient.get("/api/patients/search?page=1&pageSize=5"),
        ]);
        setStats(statsRes.data);
        setRecentAppointments(appointmentsRes.data.items || []);
        setRecentPatients(patientsRes.data.items || []);
      } catch (error) {
        console.error("Failed to fetch dashboard data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const activities = [
    "New appointment created by Ahmed Ali",
    "Mona Hassan appointment marked as completed",
    "New patient profile added",
  ];

  if (loading) {
    return <div className="dashboard-page">Loading...</div>;
  }

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
          <span>{stats?.totalAppointmentsToday || 0}</span>
        </div>
        <div className="stat-card">
          <h3>Patients Today</h3>
          <span>{stats?.totalPatients || 0}</span>
        </div>
        <div className="stat-card">
          <h3>Completed</h3>
          <span>{stats?.confirmedAppointments || 0}</span>
        </div>
        <div className="stat-card">
          <h3>Cancelled</h3>
          <span>0</span>
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
          {recentAppointments.length > 0 ? (
            recentAppointments.map((item) => (
              <div className="mini-row" key={item.id}>
                <div>
                  <strong>{item.patientName}</strong>
                  <p>
                    {item.doctorName} • {item.startTime}
                  </p>
                </div>
                <span>{item.status}</span>
              </div>
            ))
          ) : (
            <p>No recent appointments.</p>
          )}
        </div>

        <div className="dashboard-section">
          <h2>Recent Patients</h2>
          {recentPatients.length > 0 ? (
            recentPatients.map((item) => (
              <div className="mini-row" key={item.id}>
                <div>
                  <strong>{item.fullName}</strong>
                  <p>{item.phone}</p>
                </div>
                <span>{item.isActive ? "Active" : "Inactive"}</span>
              </div>
            ))
          ) : (
            <p>No recent patients.</p>
          )}
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
