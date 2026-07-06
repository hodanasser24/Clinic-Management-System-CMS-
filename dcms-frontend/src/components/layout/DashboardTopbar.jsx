import "./DashboardTopbar.css";

function DashboardTopbar() {
  return (
    <header className="dashboard-topbar">
      <div></div>

      <div className="topbar-actions">
        <div className="search-box">🔍 Search...</div>
        <button className="icon-btn">🔔</button>
        <div className="user-avatar">H</div>
      </div>
    </header>
  );
}

export default DashboardTopbar;
