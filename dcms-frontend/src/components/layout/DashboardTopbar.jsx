import { useLocation, useNavigate } from "react-router-dom";
import "./DashboardTopbar.css";

function DashboardTopbar() {
  const navigate = useNavigate();
  const { pathname } = useLocation();

  const goToNotifications = () => {
    if (pathname.startsWith("/doctor")) {
      navigate("/doctor/notifications");
    } else if (pathname.startsWith("/patient")) {
      navigate("/patient/notifications");
    } else {
      navigate("/moderator/notifications");
    }
  };

  return (
    <header className="dashboard-topbar">
      <div></div>

      <div className="topbar-actions">
        <div className="search-box">🔍 Search...</div>

        <button
          className="icon-btn"
          onClick={goToNotifications}
          title="Notifications"
        >
          🔔
        </button>

        <div className="user-avatar">H</div>
      </div>
    </header>
  );
}

export default DashboardTopbar;
