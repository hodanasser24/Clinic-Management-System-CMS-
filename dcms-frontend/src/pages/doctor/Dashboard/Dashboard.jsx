import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import "./Dashboard.css";

function Dashboard() {
  const navigate = useNavigate();
  const [stats, setStats] = useState(null);
  const [schedule, setSchedule] = useState([]);
  const [notes, setNotes] = useState([]);
  const [newNote, setNewNote] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const doctorId = getUserId();
        const [statsRes, scheduleRes, notesRes] = await Promise.all([
          apiClient.get("/api/Dashboard/doctor/daily"),
          apiClient.get(
            `/api/Appointment/by-doctor/${doctorId}?page=1&pageSize=10`,
          ),
          apiClient.get("/api/DoctorNote"),
        ]);
        setStats(statsRes.data);
        setSchedule(scheduleRes.data.items || []);
        setNotes(notesRes.data || []);
      } catch (error) {
        console.error("Failed to fetch doctor dashboard data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleAddNote = async () => {
    if (!newNote.trim()) return;
    try {
      const res = await apiClient.post("/api/DoctorNote", { content: newNote });
      setNotes([res.data, ...notes]);
      setNewNote("");
    } catch (error) {
      console.error("Failed to add note:", error);
    }
  };

  const handleDeleteNote = async (id) => {
    try {
      await apiClient.delete(`/api/DoctorNote/${id}`);
      setNotes(notes.filter((n) => n.id !== id));
    } catch (error) {
      console.error("Failed to delete note:", error);
    }
  };

  if (loading) {
    return <div className="doctor-dashboard-page">Loading...</div>;
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
          <h3>Pending Cases</h3>
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
        <button onClick={() => navigate("/doctor/prescriptions")}>
          Prescriptions
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
                    {item.startTime} • {item.serviceName}
                  </p>
                </div>

                <span
                  className={`doctor-status ${(item.status || "").toLowerCase()}`}
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

          <div className="review-box">
            <h3>Cases Need Follow Up</h3>
            <span>{stats?.pendingAppointments || 0}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
