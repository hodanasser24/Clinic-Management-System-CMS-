import { useState, useEffect } from "react";
import apiClient from "../../../services/apiClient";
import "./Reports.css";

function Reports() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSummary = async () => {
      try {
        const res = await apiClient.get("/api/Dashboard/summary");
        setSummary(res.data);
      } catch (err) {
        console.error("Failed to fetch dashboard summary", err);
      } finally {
        setLoading(false);
      }
    };
    fetchSummary();
  }, []);

  const handleExportPDF = async () => {
    try {
      const today = new Date().toISOString().split("T")[0];
      const res = await apiClient.get(`/api/Dashboard/daily/export?date=${today}`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `daily-report-${today}.csv`);
      document.body.appendChild(link);
      link.click();
      link.parentNode.removeChild(link);
    } catch (err) {
      console.error("Failed to export report", err);
      alert("Failed to export report.");
    }
  };

  return (
    <div className="reports-page">
      <div className="reports-header">
        <div>
          <h1>Reports</h1>
          <p>Clinic statistics and performance overview.</p>
        </div>

        <button onClick={handleExportPDF}>Export CSV</button>
      </div>

      {loading ? (
        <p>Loading reports...</p>
      ) : (
        <div className="report-cards">
          <div className="report-card">
            <h3>Today's Appointments</h3>
            <span>{summary?.todayAppointments ?? summary?.totalAppointments ?? 0}</span>
          </div>

          <div className="report-card">
            <h3>Completed</h3>
            <span>{summary?.completedAppointments ?? 0}</span>
          </div>

          <div className="report-card">
            <h3>Cancelled</h3>
            <span>{summary?.cancelledAppointments ?? 0}</span>
          </div>

          <div className="report-card">
            <h3>Revenue</h3>
            <span>{summary?.totalRevenue ?? 0} EGP</span>
          </div>
        </div>
      )}

      <div className="chart-placeholder">Charts will be connected later.</div>
    </div>
  );
}

export default Reports;
