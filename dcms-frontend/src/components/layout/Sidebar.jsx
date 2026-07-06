import { NavLink, useLocation, useNavigate } from "react-router-dom";
import { logout } from "../../services/authServices";
import "./Sidebar.css";

// Isolate role detection logic to make it easy to migrate to Context/Auth token later.
function getCurrentRole(pathname) {
  if (pathname.startsWith("/patient")) {
    return "patient";
  }
  return "moderator";
}

function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const role = getCurrentRole(location.pathname);

  async function handleLogout() {
    await logout();
    navigate("/login");
  }

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <div className="sidebar-logo">🦷</div>

        <div>
          <h2>DCMS</h2>
          <p>{role === "patient" ? "Patient Portal" : "Moderator Portal"}</p>
        </div>
      </div>

      <nav className="sidebar-nav">
        {role === "moderator" ? (
          <>
            <NavLink to="/moderator/dashboard">🏠 Dashboard</NavLink>
            <NavLink to="/moderator/appointments">📅 Appointments</NavLink>
            <NavLink to="/moderator/patients">👥 Patients</NavLink>
            <NavLink to="/moderator/reports">📊 Reports</NavLink>
            <NavLink to="/moderator/notifications">🔔 Notifications</NavLink>
            <NavLink to="/moderator/profile">👤 Profile</NavLink>
          </>
        ) : (
          <>
            <NavLink to="/patient/dashboard">🏠 Dashboard</NavLink>
            <NavLink to="/patient/appointments">📅 My Appointments</NavLink>
            <NavLink to="/patient/dental-chart">🦷 Dental Chart</NavLink>
            <NavLink to="/patient/notifications">🔔 Notifications</NavLink>
            <NavLink to="/patient/profile">👤 Profile</NavLink>
          </>
        )}
      </nav>

      <div className="sidebar-footer">
        <p>Need help?</p>
        <button onClick={() => alert("Contact support")}>Contact Support</button>
        <button onClick={handleLogout} style={{ marginTop: "10px", background: "rgba(239, 68, 68, 0.1)", color: "#ef4444", borderColor: "rgba(239, 68, 68, 0.2)" }}>Logout</button>
      </div>
    </aside>
  );
}

export default Sidebar;
