import { useState, useEffect } from "react";
import apiClient from "../../../services/apiClient";
import DataTable from "../../../components/common/DataTable/DataTable";
import "./Reports.css";

function Reports() {
  const [summary, setSummary] = useState(null);
  const [dailyReports, setDailyReports] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError("");
        const today = new Date().toISOString().split("T")[0];
        const [summaryRes, dailyRes] = await Promise.all([
          apiClient.get("/api/Dashboard/summary"),
          apiClient.get(`/api/Dashboard/daily?date=${today}`)
        ]);
        setSummary(summaryRes.data);
        setDailyReports((dailyRes.data.appointments || []).map(a => ({ ...a, id: a.appointmentId })));
      } catch (err) {
        console.error("Failed to fetch reports", err);
        const errData = err.response?.data;
        let msg = "Failed to load reports.";
        if (errData) {
          if (errData.errors) msg = Object.values(errData.errors).flat().join("\n");
          else if (errData.message) msg = errData.message;
          else if (errData.detail) msg = errData.detail;
          else if (typeof errData === "string") msg = errData;
        } else if (err.message) {
          msg = err.message;
        }
        setError(msg);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleExportPDF = async () => {
    try {
      const today = new Date().toISOString().split("T")[0];
      const res = await apiClient.get(`/api/Dashboard/daily/export?date=${today}`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement("a");
      link.href = url;
      
      let filename = `daily-report-${today}.csv`;
      const disposition = res.headers['content-disposition'];
      if (disposition && disposition.indexOf('filename=') !== -1) {
        const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
        if (matches != null && matches[1]) {
          filename = matches[1].replace(/['"]/g, '');
        }
      }
      
      link.setAttribute("download", filename);
      document.body.appendChild(link);
      link.click();
      link.parentNode.removeChild(link);
    } catch (err) {
      console.error("Failed to export report", err);
      const errData = err.response?.data;
      alert(errData?.message || errData?.detail || "Failed to export report.");
    }
  };

  const columns = [
    { key: "appointmentId", label: "Appointment ID" },
    { key: "patientName", label: "Patient" },
    { key: "doctorName", label: "Doctor" },
    { key: "startTime", label: "Date / Time" },
    { key: "status", label: "Status" },
    { key: "serviceName", label: "Medical Info (Service)" }
  ];

  return (
    <div className="reports-page">
      <div className="reports-header">
        <div>
          <h1>Reports</h1>
          <p>Clinic statistics and performance overview.</p>
        </div>

        <button onClick={handleExportPDF}>Export CSV</button>
      </div>

      {error && <div style={{ color: "red", marginBottom: "1rem" }}>{error}</div>}

      {loading ? (
        <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading reports...</div>
      ) : (
        <>
          <div className="report-cards">
            <div className="report-card">
              <h3>Today's Appointments</h3>
              <span>{summary?.totalAppointmentsToday ?? summary?.todayAppointments ?? 0}</span>
            </div>

            <div className="report-card">
              <h3>Completed</h3>
              <span>{dailyReports.filter(a => a.status === "Completed").length}</span>
            </div>

            <div className="report-card">
              <h3>Pending</h3>
              <span>{dailyReports.filter(a => a.status === "Pending").length}</span>
            </div>

            <div className="report-card">
              <h3>Revenue</h3>
              <span>{summary?.todayRevenue ?? summary?.totalRevenue ?? 0} EGP</span>
            </div>
          </div>

          <div className="dashboard-section" style={{ marginTop: '2rem' }}>
            <h2>Daily Report Details</h2>
            {dailyReports.length > 0 ? (
              <DataTable
                columns={columns}
                data={dailyReports}
              />
            ) : (
              <p>No reports available for today.</p>
            )}
          </div>
        </>
      )}
    </div>
  );
}

export default Reports;
