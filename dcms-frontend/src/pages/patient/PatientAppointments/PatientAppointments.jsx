import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./PatientAppointments.css";

function PatientAppointments() {
  const navigate = useNavigate();

  const [appointments] = useState([
    {
      id: 1,
      doctor: "Dr. Sara Ahmed",
      department: "Orthodontics",
      date: "06 Jul 2026",
      time: "10:30 AM",
      status: "Confirmed",
    },
    {
      id: 2,
      doctor: "Dr. Mohamed Ali",
      department: "Dental Surgery",
      date: "15 Jul 2026",
      time: "01:00 PM",
      status: "Pending",
    },
    {
      id: 3,
      doctor: "Dr. Nada Hassan",
      department: "Cleaning",
      date: "22 Jun 2026",
      time: "11:00 AM",
      status: "Completed",
    },
  ]);

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");

  // Filtering
  let filtered = appointments.filter(
    (app) =>
      app.doctor.toLowerCase().includes(searchQuery.toLowerCase()) ||
      app.department.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (statusFilter !== "All") {
    if (statusFilter === "Upcoming") {
      filtered = filtered.filter((app) =>
        ["Confirmed", "Pending"].includes(app.status)
      );
    } else {
      filtered = filtered.filter((app) => app.status === statusFilter);
    }
  }

  return (
    <div className="patient-appointments-page">
      <div className="page-header">
        <h1>My Appointments</h1>
        <button onClick={() => alert("Opening booking wizard...")}>
          + Book Appointment
        </button>
      </div>

      <div className="toolbar">
        <input
          type="text"
          placeholder="Search doctor..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="All">All</option>
          <option value="Upcoming">Upcoming</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      <table className="appointments-table">
        <thead>
          <tr>
            <th>Doctor</th>
            <th>Department</th>
            <th>Date</th>
            <th>Time</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          {filtered.map((item) => (
            <tr key={item.id}>
              <td>{item.doctor}</td>

              <td>{item.department}</td>

              <td>{item.date}</td>

              <td>{item.time}</td>

              <td>
                <span className={item.status.toLowerCase()}>{item.status}</span>
              </td>

              <td>
                <button
                  onClick={() => navigate(`/patient/appointments/${item.id}`)}
                >
                  View
                </button>

                <button
                  className="cancel"
                  onClick={() => alert("Appointment cancelled.")}
                >
                  Cancel
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default PatientAppointments;
