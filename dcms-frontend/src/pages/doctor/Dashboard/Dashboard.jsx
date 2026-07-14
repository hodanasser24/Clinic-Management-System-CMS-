import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import "./Dashboard.css";
import { getFriendlyErrorMessage } from "../../../utils/errorMapper";

function Dashboard() {
  const navigate = useNavigate();
  const [stats, setStats] = useState(null);
  const [schedule, setSchedule] = useState([]);
  const [notes, setNotes] = useState([]);
  const [newNote, setNewNote] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [noteError, setNoteError] = useState("");

  const fetchData = async () => {
    try {
      const doctorId = getUserId();
      const todayStr = new Date().toISOString().split('T')[0];
      const results = await Promise.allSettled([
        apiClient.get("/api/Dashboard/doctor/daily"),
        apiClient.get(`/api/Appointment/by-doctor/${doctorId}?FromDate=${todayStr}&ToDate=${todayStr}&page=1&pageSize=100`),
        apiClient.get("/api/DoctorNote"),
      ]);

      if (results[0].status === "fulfilled") setStats(results[0].value.data);
      if (results[1].status === "fulfilled") {
        const validAppointments = (results[1].value.data.items || [])
          .filter(a => a.status === "Pending" || a.status === "Confirmed" || a.status === "Completed")
          .sort((a, b) => {
             const timeA = a.startTime ? a.startTime.split(':').join('') : '999999';
             const timeB = b.startTime ? b.startTime.split(':').join('') : '999999';
             return timeA.localeCompare(timeB);
          });
        setSchedule(validAppointments);
      }
      if (results[2].status === "fulfilled") setNotes(results[2].value.data || []);
      
      const failed = results.filter(r => r.status === "rejected");
      if (failed.length > 0) {
        console.error("Some widgets failed to load", failed.map(r => r.reason));
      }
    } catch (err) {
      setError(getFriendlyErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleAddNote = async () => {
    if (!newNote.trim()) return;
    setNoteError("");
    try {
      await apiClient.post("/api/DoctorNote", { content: newNote.trim() });
      setNewNote("");
      await fetchData();
    } catch (err) {
      setNoteError(getFriendlyErrorMessage(err));
    }
  };

  const handleDeleteNote = async (id) => {
    setNoteError("");
    try {
      await apiClient.delete(`/api/DoctorNote/${id}`);
      await fetchData();
    } catch (err) {
      setNoteError(getFriendlyErrorMessage(err));
    }
  };

  if (loading) {
    return <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading...</div>;
  }

  return (
    <div className="doctor-dashboard-page">
      <div className="doctor-dashboard-header">
        <div>
          <h1>Doctor Dashboard</h1>
          <p>Welcome back! Here's your schedule and patient overview.</p>
        </div>

        <button onClick={() => navigate("/doctor/reports")}>
          View Reports
        </button>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="doctor-stats-cards">
        <div className="doctor-stat-card">
          <h3>Today's Appointments</h3>
          <span>{stats?.totalAppointments || 0}</span>
        </div>

        <div className="doctor-stat-card">
          <h3>Completed Cases</h3>
          <span>{stats?.completedAppointments || 0}</span>
        </div>

        <div className="doctor-stat-card">
          <h3>Waiting Patients</h3>
          <span>{stats?.pendingAppointments || 0}</span>
        </div>

        <div className="doctor-stat-card">
          <h3>Prescriptions Today</h3>
          <span>{stats?.prescriptionsCreated || 0}</span>
        </div>
      </div>

      <div className="doctor-quick-actions">
        <button onClick={() => navigate("/doctor/appointments")}>
          Appointments
        </button>
        <button onClick={() => navigate("/doctor/patients")}>Patients</button>
        <button onClick={() => navigate("/doctor/medical-records")}>
          Medical Records
        </button>
      </div>

      <div className="doctor-dashboard-grid">
        <div className="doctor-section">
          <h2>Today's Schedule</h2>

          {schedule.length > 0 ? (
            schedule.map((item) => (
              <div className="doctor-mini-row" key={item.id}>
                <div>
                  <strong>{item.patientName}</strong>
                  <p>
                    {formatTo12Hour(item.startTime)} - {formatTo12Hour(item.endTime)}
                  </p>
                </div>

                <span
                  className={`status-badge ${item.status.toLowerCase()}`}
                >
                  {item.status}
                </span>
              </div>
            ))
          ) : (
            <p>No appointments today.</p>
          )}
        </div>

        <div className="doctor-section doctor-notes-section">
          <h2>Today's Notes</h2>

          {noteError && <p className="error">{noteError}</p>}

          <div className="doctor-note-input-container">
            <input
              type="text"
              placeholder="Add a new note..."
              value={newNote}
              onChange={(e) => setNewNote(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleAddNote()}
            />
            <button onClick={handleAddNote}>Add</button>
          </div>

          <div className="doctor-notes-list">
            {notes.map((note) => (
              <div className="doctor-note" key={note.id}>
                <span>📝 {note.content}</span>
                <button
                  className="delete-note-btn"
                  onClick={() => handleDeleteNote(note.id)}
                  title="Delete Note"
                >
                  ✕
                </button>
              </div>
            ))}
            {notes.length === 0 && (
              <p className="no-notes">No notes available.</p>
            )}
          </div>
        </div>
      </div>

      <div className="doctor-section">
        <h2>Daily Review</h2>

        <div className="doctor-review-grid">
          <div className="review-box">
            <h3>Medical Records Updated</h3>
            <span>{stats?.reportsCreated || 0}</span>
          </div>

          <div className="review-box">
            <h3>Prescriptions Created</h3>
            <span>{stats?.prescriptionsCreated || 0}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
