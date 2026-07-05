import "./Dashboard.css";

function Dashboard() {
  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <h1>Moderator Dashboard</h1>
        <p>Welcome back! Here's today's clinic overview.</p>
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

      <div className="dashboard-section">
        <h2>Recent Appointments</h2>

        <div className="placeholder-box">
          Appointments Table will be added here.
        </div>
      </div>

      <div className="dashboard-section">
        <h2>Recent Patients</h2>

        <div className="placeholder-box">
          Patients Table will be added here.
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
