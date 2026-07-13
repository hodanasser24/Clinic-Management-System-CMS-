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
    } else if (pathname.startsWith("/owner")) {
      navigate("/owner/notifications");
    } else {
      navigate("/moderator/notifications");
    }
  };

  const goHome = () => {
    if (pathname.startsWith("/doctor")) {
      navigate("/doctor/dashboard");
    } else if (pathname.startsWith("/patient")) {
      navigate("/patient/dashboard");
    } else if (pathname.startsWith("/owner")) {
      navigate("/owner/dashboard");
    } else {
      navigate("/moderator/dashboard");
    }
  };

  return (
    <header className="dashboard-topbar">
      <div></div>

      <div className="topbar-actions">
        <button
          className="icon-btn"
          onClick={goHome}
          title="Home"
        >
          🏠
        </button>

        <button
          className="icon-btn"
          onClick={goToNotifications}
          title="Notifications"
        >
          🔔
        </button>

      </div>
    </header>
  );
}

export default DashboardTopbar;
