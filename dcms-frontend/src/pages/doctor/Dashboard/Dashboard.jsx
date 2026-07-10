import { useNavigate } from "react-router-dom";
import "./Dashboard.css";

function Dashboard() {
  const navigate = useNavigate();

  const schedule = [
    {
      id: 1,
      patient: "Ahmed Ali",
      time: "10:00 AM",
      service: "Teeth Cleaning",
      status: "Pending",
    },
    {
      id: 2,
      patient: "Mona Hassan",
      time: "11:30 AM",
      service: "Root Canal",
      status: "Confirmed",
    },
    {
      id: 3,
      patient: "Omar Mohamed",
      time: "01:00 PM",
      service: "Consultation",
      status: "Completed",
    },
  ];

  const notes = [
    "Review Ahmed Ali X-ray before treatment.",
    "Follow up Mona Hassan after root canal.",
    "Complete Omar Mohamed medical report.",
  ];

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
          <span>14</span>
        </div>

        <div className="doctor-stat-card">
          <h3>Completed Cases</h3>
          <span>9</span>
        </div>

        <div className="doctor-stat-card">
          <h3>Pending Cases</h3>
          <span>5</span>
        </div>

        <div className="doctor-stat-card">
          <h3>Prescriptions Today</h3>
          <span>7</span>
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

          {schedule.map((item) => (
            <div className="doctor-mini-row" key={item.id}>
              <div>
                <strong>{item.patient}</strong>
                <p>
                  {item.time} • {item.service}
                </p>
              </div>

              <span className={`doctor-status ${item.status.toLowerCase()}`}>
                {item.status}
              </span>
            </div>
          ))}
        </div>

        <div className="doctor-section">
          <h2>Today's Notes</h2>

          {notes.map((note, index) => (
            <div className="doctor-note" key={index}>
              📝 {note}
            </div>
          ))}
        </div>
      </div>

      <div className="doctor-section">
        <h2>Daily Review</h2>

        <div className="doctor-review-grid">
          <div className="review-box">
            <h3>Medical Records Updated</h3>
            <span>6</span>
          </div>

          <div className="review-box">
            <h3>Prescriptions Created</h3>
            <span>7</span>
          </div>

          <div className="review-box">
            <h3>Cases Need Follow Up</h3>
            <span>4</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
