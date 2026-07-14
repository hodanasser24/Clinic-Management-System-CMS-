import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getUserId, getUserName } from "../../../services/authServices";
import { getPatientProfile } from "../../../services/profileServices";
import { getUpcomingPatientAppointments, getPatientAppointments } from "../../../services/appointmentServices";
import { getPatientPrescriptions } from "../../../services/prescriptionServices";
import { getNotifications } from "../../../services/notificationServices";
import "./PatientDashboard.css";

function PatientDashboard() {
  const navigate = useNavigate();
  const userId = getUserId();
  const [profile, setProfile] = useState(null);
  const [upcoming, setUpcoming] = useState([]);
  const [historyCount, setHistoryCount] = useState(0);
  const [prescriptionCount, setPrescriptionCount] = useState(0);
  const [notificationCount, setNotificationCount] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadDashboardData() {
      try {
        const [profileData, upcomingData, historyData, prescriptionsData, notificationsData] = await Promise.all([
          getPatientProfile(),
          getUpcomingPatientAppointments(userId, { page: 1, pageSize: 5 }),
          getPatientAppointments(userId, { status: 4, page: 1, pageSize: 1 }), // 4 = Completed
          getPatientPrescriptions(userId, { page: 1, pageSize: 1 }).catch(() => ({ totalCount: 0 })),
          getNotifications({ unreadOnly: true }).catch(() => ({ unreadCount: 0 }))
        ]);
        setProfile(profileData);
        setUpcoming(upcomingData?.items || upcomingData || []);
        setHistoryCount(historyData?.totalCount || 0);
        setPrescriptionCount(prescriptionsData?.totalCount || 0);
        setNotificationCount(notificationsData?.unreadCount || 0);
      } catch (err) {
        console.error("Error loading patient dashboard data:", err);
      } finally {
        setLoading(false);
      }
    }
    loadDashboardData();
  }, [userId]);

  const welcomeName = profile?.fullName || getUserName();
  const nextAppt = upcoming[0];

  return (
    <div className="patient-dashboard-page">
      <div className="patient-header">
        <div>
          <h1>Welcome, {welcomeName} 👋</h1>
          <p>Manage your appointments and medical records easily.</p>
        </div>

        <button onClick={() => navigate("/patient/profile")}>My Profile</button>
      </div>

      {loading ? (
        <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>
          Loading dashboard...
        </div>
      ) : (
        <>
          <div className="patient-stats">
            <div className="patient-card">
              <h3>Upcoming Appointments</h3>
              <span>{upcoming.length}</span>
            </div>

            <div className="patient-card">
              <h3>Completed Visits</h3>
              <span>{historyCount}</span>
            </div>

            <div className="patient-card">
              <h3>Prescriptions</h3>
              <span>{prescriptionCount}</span>
            </div>

            <div className="patient-card" onClick={() => navigate("/patient/notifications")} style={{ cursor: "pointer" }}>
              <h3>Notifications</h3>
              <span>{notificationCount}</span>
            </div>
          </div>

          <div className="patient-grid">
            <div className="patient-section">
              <h2>Next Appointment</h2>

              {nextAppt ? (
                <div className="appointment-box">
                  <h3>Dr. {nextAppt.doctorName}</h3>
                  <p>{nextAppt.serviceName}</p>
                  <p>{nextAppt.appointmentDate}</p>
                  <p>{nextAppt.startTime}</p>
                  <span className={`status ${nextAppt.status.toLowerCase()}`}>
                    {nextAppt.status}
                  </span>
                  <button onClick={() => navigate("/patient/appointments")}>
                    View Details
                  </button>
                </div>
              ) : (
                <div className="appointment-box" style={{ textAlign: "center", padding: "2rem" }}>
                  <p style={{ color: "var(--text-secondary)", marginBottom: "1rem" }}>
                    No upcoming appointments scheduled.
                  </p>
                  <button onClick={() => navigate("/patient/appointments")}>
                    Book Appointment
                  </button>
                </div>
              )}
            </div>

            <div className="patient-section">
              <h2>Medical Summary</h2>

              <p>
                <strong>Date of Birth:</strong> {profile?.dateOfBirth || "Not specified"}
              </p>
              <p>
                <strong>Gender:</strong> {profile?.gender || "Not specified"}
              </p>
              <p>
                <strong>Blood Type:</strong> {profile?.bloodType || "Not specified"}
              </p>
              <p>
                <strong>Allergies:</strong> {profile?.allergies || "None specified"}
              </p>
              <p>
                <strong>Contact Phone:</strong> {profile?.phone || "Not specified"}
              </p>
              <p>
                <strong>Email Address:</strong> {profile?.email || "Not specified"}
              </p>
              <p>
                <strong>Medical History:</strong> {profile?.medicalHistory || "None specified"}
              </p>
            </div>
          </div>
        </>
      )}

      <div className="patient-section" style={{ marginTop: "2rem" }}>
        <h2>Quick Actions</h2>

        <div className="quick-buttons">
          <button onClick={() => navigate("/patient/appointments")}>
            My Appointments
          </button>

          <button onClick={() => navigate("/patient/medical-records")}>
            Medical Records
          </button>

          <button onClick={() => navigate("/patient/prescriptions")}>
            Prescriptions
          </button>

          <button onClick={() => navigate("/patient/profile")}>Profile</button>
        </div>
      </div>
    </div>
  );
}

export default PatientDashboard;
