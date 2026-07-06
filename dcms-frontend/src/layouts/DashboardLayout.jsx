import Sidebar from "../components/layout/Sidebar";
import DashboardTopbar from "../components/layout/DashboardTopbar";
import "./DashboardLayout.css";

function DashboardLayout({ children }) {
  return (
    <div className="dashboard-layout">
      <Sidebar />

      <main className="dashboard-main">
        <DashboardTopbar />

        <div className="dashboard-content">{children}</div>
      </main>
    </div>
  );
}

export default DashboardLayout;
