import { useState, useEffect } from "react";
import apiClient from "../../../services/apiClient";
import "./Reports.css";

function Reports() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const res = await apiClient.get("/api/Dashboard/doctor/daily");
        setStats(res.data);
      } catch (err) {
        console.error("Failed to fetch doctor daily stats", err);
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  const reports = stats
    ? [
        { title: "Patients Treated Today", value: stats.patientsTreatedToday ?? stats.completedAppointments ?? 0 },
        { title: "Completed Appointments", value: stats.completedAppointments ?? 0 },
        { title: "Pending Follow-ups", value: stats.pendingAppointments ?? stats.pendingFollowUps ?? 0 },
        { title: "Prescriptions Issued", value: stats.prescriptionsIssued ?? stats.totalPrescriptions ?? 0 },
      ]
    : [];

  return (
    <div className="doctor-reports-page">
      <h1>Doctor Reports</h1>
      <p>Overview of your daily and weekly performance.</p>

      {loading ? (
        <p>Loading reports...</p>
      ) : (
        <div className="reports-grid">
          {reports.map((item) => (
            <div className="report-card" key={item.title}>
              <h3>{item.title}</h3>
              <span>{item.value}</span>
            </div>
          ))}
        </div>
      )}

      <div className="report-summary">
        <h2>Summary</h2>

        <p>
          This report summarizes your completed appointments, prescriptions and
          patient follow-ups.
        </p>
      </div>
    </div>
  );
}

export default Reports;
