import { NavLink } from "react-router-dom";
import "./Sidebar.css";

function Sidebar() {
  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <div className="sidebar-logo">🦷</div>

        <div>
          <h2>DCMS</h2>
          <p>Moderator Portal</p>
        </div>
      </div>

      <nav className="sidebar-nav">
        <NavLink to="/moderator/dashboard">🏠 Dashboard</NavLink>

        <NavLink to="/moderator/appointments">📅 Appointments</NavLink>

        <NavLink to="/moderator/patients">👥 Patients</NavLink>

        <NavLink to="/moderator/reports">📊 Reports</NavLink>

        <NavLink to="/moderator/notifications">🔔 Notifications</NavLink>

        <NavLink to="/moderator/profile">👤 Profile</NavLink>
      </nav>

      <div className="sidebar-footer">
        <p>Need help?</p>
        <button>Contact Support</button>
      </div>
    </aside>
  );
}

export default Sidebar;
