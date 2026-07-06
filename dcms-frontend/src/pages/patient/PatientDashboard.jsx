import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import DashboardLayout from "../../layouts/DashboardLayout";
import "./PatientDashboard.css";

import { getCurrentPatientId, getCurrentPatientName } from "../../services/authServices";
import { getUpcomingPatientAppointments, getHistoryPatientAppointments } from "../../services/appointmentServices";
import { getUnreadNotifications } from "../../services/notificationServices";
import { getActiveOffers } from "../../services/publicServices";

import Loading from "../../components/common/Loading/Loading";
import EmptyState from "../../components/common/EmptyState/EmptyState";

function PatientDashboard() {
  const navigate = useNavigate();
  const patientId = getCurrentPatientId();
  const patientName = getCurrentPatientName();

  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState({ upcoming: 0, history: 0, unread: 0 });
  const [nextAppointment, setNextAppointment] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [offers, setOffers] = useState([]);

  const loadDashboardData = useCallback(async () => {
    setLoading(true);

    // Fetch independently to avoid single point of failure
    const upcomingPromise = getUpcomingPatientAppointments(patientId, { page: 1, pageSize: 1 })
      .catch(err => { console.error("Upcoming fail", err); return null; });
    const historyPromise = getHistoryPatientAppointments(patientId, { page: 1, pageSize: 1 })
      .catch(err => { console.error("History fail", err); return null; });
    const notifPromise = getUnreadNotifications(3)
      .catch(err => { console.error("Notif fail", err); return null; });
    const offersPromise = getActiveOffers()
      .catch(err => { console.error("Offers fail", err); return null; });

    const [upcomingRes, historyRes, notifRes, offersRes] = await Promise.all([
      upcomingPromise,
      historyPromise,
      notifPromise,
      offersPromise
    ]);

    // Parse Stats (relying on backend TotalCount if available, else length)
    setStats({
      upcoming: upcomingRes?.totalCount ?? upcomingRes?.items?.length ?? 0,
      history: historyRes?.totalCount ?? historyRes?.items?.length ?? 0,
      unread: notifRes?.totalCount ?? notifRes?.items?.length ?? 0
    });

    // Next Appointment
    if (upcomingRes?.items?.length > 0) {
      setNextAppointment(upcomingRes.items[0]);
    }

    // Notifications (latest 3 unread)
    setNotifications(notifRes?.items || []);

    // Offers
    setOffers(offersRes || []);

    setLoading(false);
  }, [patientId]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadDashboardData();
  }, [loadDashboardData]);

  if (loading) {
    return (
      <DashboardLayout>
        <div style={{ padding: "40px" }}><Loading /></div>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout>
      <section className="patient-dashboard">
        {/* Welcome Card */}
        <div className="welcome-card">
          <div>
            <p>Good Morning, {patientName} 👋</p>
            <h2>Your dental care is managed in one place.</h2>
            <span>Track appointments, prescriptions, records and updates.</span>
          </div>
          <button onClick={() => navigate("/patient/appointments/book")}>Book Appointment</button>
        </div>

        {/* Dashboard Summary Stats */}
        <div className="stats-grid">
          <div className="stat-card">
            <span>📅</span>
            <h3>{stats.upcoming}</h3>
            <p>Upcoming Appointments</p>
          </div>
          <div className="stat-card">
            <span>📂</span>
            <h3>{stats.history}</h3>
            <p>Past Appointments</p>
          </div>
          <div className="stat-card">
            <span>🔔</span>
            <h3>{stats.unread}</h3>
            <p>Unread Notifications</p>
          </div>
        </div>

        <div className="dashboard-grid">
          {/* Next Appointment Section */}
          <div className="dashboard-panel">
            <h3>Next Appointment</h3>
            {nextAppointment ? (
              <div className="appointment-card">
                <div>
                  <strong>{nextAppointment.doctorName || "Doctor"}</strong>
                  <p>{nextAppointment.serviceName}</p>
                  <p style={{ fontSize: "12px", color: "#64748b" }}>{nextAppointment.branchName}</p>
                </div>
                <div style={{ textAlign: "right" }}>
                  <span style={{ display: "block", marginBottom: "8px" }}>
                    {nextAppointment.date} &middot; {nextAppointment.startTime?.substring(0, 5)}
                  </span>
                  <span className={`status-badge ${nextAppointment.status?.toLowerCase()}`}>
                    {nextAppointment.status}
                  </span>
                </div>
                <div style={{ width: "100%", marginTop: "12px", display: "flex", justifyContent: "flex-end" }}>
                  <button 
                    onClick={() => navigate(`/patient/appointments/${nextAppointment.id}`)}
                    style={{ padding: "6px 12px", fontSize: "13px", borderRadius: "6px", background: "none", border: "1px solid #94a3b8", cursor: "pointer" }}
                  >
                    View Details
                  </button>
                </div>
              </div>
            ) : (
              <div style={{ marginTop: "16px" }}>
                <EmptyState 
                  title="No Upcoming Appointments" 
                  message="You don't have any appointments scheduled right now." 
                />
                <button 
                  onClick={() => navigate("/patient/appointments/book")}
                  style={{ marginTop: "16px", background: "linear-gradient(135deg, #2f6bff, #6d3df5)", color: "white", padding: "10px 20px", borderRadius: "8px", border: "none", cursor: "pointer", width: "100%", fontWeight: "600" }}
                >
                  Book Your First Appointment
                </button>
              </div>
            )}
          </div>

          {/* Quick Actions */}
          <div className="dashboard-panel">
            <h3>Quick Actions</h3>
            <div className="quick-actions">
              <button onClick={() => navigate("/patient/appointments/book")}>Book Appointment</button>
              <button onClick={() => navigate("/patient/appointments")}>My Appointments</button>
              <button onClick={() => navigate("/patient/profile")}>Update Profile</button>
            </div>
          </div>
        </div>

        {/* Secondary Grid for Notifications and Offers */}
        <div className="dashboard-grid" style={{ marginTop: "24px" }}>
          
          {/* Notifications Preview */}
          <div className="dashboard-panel">
            <h3>Recent Notifications</h3>
            <div style={{ marginTop: "16px", display: "flex", flexDirection: "column", gap: "12px" }}>
              {notifications.length > 0 ? (
                <>
                  {notifications.map((notif, idx) => (
                    <div key={idx} style={{ padding: "12px", background: "rgba(15, 23, 42, 0.4)", borderRadius: "12px", border: "1px solid rgba(148, 163, 184, 0.1)" }}>
                      <strong style={{ display: "block", color: "#e2e8f0", marginBottom: "4px" }}>{notif.title || "Notification"}</strong>
                      <p style={{ color: "#94a3b8", fontSize: "13px", margin: 0 }}>{notif.message}</p>
                      <span style={{ fontSize: "11px", color: "#64748b", marginTop: "8px", display: "block" }}>{new Date(notif.createdAt).toLocaleDateString()}</span>
                    </div>
                  ))}
                  <button 
                    style={{ background: "none", border: "none", color: "#60a5fa", cursor: "pointer", textAlign: "left", padding: "4px 0", marginTop: "8px" }}
                  >
                    View All Notifications &rarr;
                  </button>
                </>
              ) : (
                <EmptyState title="All Caught Up!" message="You have no unread notifications." />
              )}
            </div>
          </div>

          {/* Active Offers */}
          <div className="dashboard-panel">
            <h3>Active Clinic Offers</h3>
            <div style={{ marginTop: "16px", display: "flex", flexDirection: "column", gap: "12px" }}>
              {offers.length > 0 ? (
                offers.slice(0, 3).map((offer, idx) => (
                  <div key={idx} style={{ padding: "16px", background: "linear-gradient(135deg, rgba(47, 107, 255, 0.1), rgba(109, 61, 245, 0.1))", borderRadius: "12px", border: "1px solid rgba(109, 61, 245, 0.2)" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                      <strong style={{ color: "#f8fafc", fontSize: "15px" }}>{offer.title}</strong>
                      {offer.discountPercentage && (
                        <span style={{ background: "#10b981", color: "white", padding: "2px 8px", borderRadius: "12px", fontSize: "12px", fontWeight: "600" }}>
                          {offer.discountPercentage}% OFF
                        </span>
                      )}
                    </div>
                    <p style={{ color: "#cbd5e1", fontSize: "13px", margin: "8px 0" }}>{offer.description}</p>
                    {(offer.serviceName || offer.branchName) && (
                      <p style={{ color: "#94a3b8", fontSize: "12px", margin: 0 }}>
                        {offer.serviceName && `Service: ${offer.serviceName}`}
                        {offer.serviceName && offer.branchName && " | "}
                        {offer.branchName && `Branch: ${offer.branchName}`}
                      </p>
                    )}
                  </div>
                ))
              ) : (
                <EmptyState title="No Active Offers" message="Check back later for new discounts and clinic offers!" />
              )}
            </div>
          </div>
        </div>

      </section>
    </DashboardLayout>
  );
}

export default PatientDashboard;
