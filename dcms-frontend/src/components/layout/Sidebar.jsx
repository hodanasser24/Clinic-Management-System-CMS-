import { NavLink, useLocation } from "react-router-dom";
import "./Sidebar.css";

function Sidebar() {
  const { pathname } = useLocation();
  const isDoctor = pathname.startsWith("/doctor");

  const title = isDoctor ? "Doctor Portal" : "Moderator Portal";

  const links = isDoctor
    ? [
        ["🏠 Dashboard", "/doctor/dashboard"],
        ["📅 Appointments", "/doctor/appointments"],
        ["👥 Patients", "/doctor/patients"],
        ["🦷 Medical Records", "/doctor/medical-records"],
        ["💊 Prescriptions", "/doctor/prescriptions"],
        ["📊 Reports", "/doctor/reports"],
        ["🔔 Notifications", "/doctor/notifications"],
        ["👤 Profile", "/doctor/profile"],
      ]
    : [
        ["🏠 Dashboard", "/moderator/dashboard"],
        ["📅 Appointments", "/moderator/appointments"],
        ["👥 Patients", "/moderator/patients"],
        ["📊 Reports", "/moderator/reports"],
        ["🔔 Notifications", "/moderator/notifications"],
        ["👤 Profile", "/moderator/profile"],
      ];

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <div className="sidebar-logo">🦷</div>
        <div>
          <h2>DCMS</h2>
          <p>{title}</p>
        </div>
      </div>

      <nav className="sidebar-nav">
        {links.map(([label, path]) => (
          <NavLink key={path} to={path}>
            {label}
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <p>Need help?</p>
        <button>Contact Support</button>
      </div>
    </aside>
  );
}

export default Sidebar;
