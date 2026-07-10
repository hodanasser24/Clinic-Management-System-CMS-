import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import "./Sidebar.css";

function Sidebar() {
  const { pathname } = useLocation();
  const [isSupportOpen, setIsSupportOpen] = useState(false);
  const [supportForm, setSupportForm] = useState({ subject: "", message: "" });
  const [supportSent, setSupportSent] = useState(false);

  const isDoctor = pathname.startsWith("/doctor");
  const isPatient = pathname.startsWith("/patient");

  let title = "Moderator Portal";

  if (isDoctor) title = "Doctor Portal";
  if (isPatient) title = "Patient Portal";

  const links = isDoctor
    ? [
        ["🏠 Dashboard", "/doctor/dashboard"],
        ["📅 Appointments", "/doctor/appointments"],
        ["👥 Patients", "/doctor/patients"],
        ["🦷 Medical Records", "/doctor/medical-records"],
        ["💊 Prescriptions", "/doctor/prescriptions"],
        ["📊 Reports", "/doctor/reports"],
        ["🔔 Notifications", "/doctor/notifications"],
        ["👤 Profile", "/doctor/profile"],
      ]
    : isPatient
      ? [
          ["🏠 Dashboard", "/patient/dashboard"],
          ["📅 My Appointments", "/patient/appointments"],
          ["🦷 Medical Records", "/patient/medical-records"],
          ["💊 Prescriptions", "/patient/prescriptions"],
          ["🔔 Notifications", "/patient/notifications"],
          ["👤 Profile", "/patient/profile"],
        ]
      : [
          ["🏠 Dashboard", "/moderator/dashboard"],
          ["📅 Appointments", "/moderator/appointments"],
          ["👥 Patients", "/moderator/patients"],
          ["📊 Reports", "/moderator/reports"],
          ["🔔 Notifications", "/moderator/notifications"],
          ["👤 Profile", "/moderator/profile"],
        ];

  const handleSupportSubmit = (e) => {
    e.preventDefault();
    if (!supportForm.subject || !supportForm.message) return;
    setSupportSent(true);
    setTimeout(() => {
      setIsSupportOpen(false);
      setSupportSent(false);
      setSupportForm({ subject: "", message: "" });
    }, 2000);
  };

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <div className="sidebar-logo">🦷</div>
        <div>
          <h2>DCMS</h2>
          <p>{title}</p>
        </div>
      </div>

      <nav className="sidebar-nav">
        {links.map(([label, path]) => (
          <NavLink key={path} to={path}>
            {label}
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <p>Need help?</p>
        <button type="button" onClick={() => setIsSupportOpen(true)}>
          Contact Support
        </button>
      </div>

      {isSupportOpen && (
        <div
          className="support-modal-overlay"
          onClick={() => setIsSupportOpen(false)}
        >
          <div
            className="support-modal-card"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="support-modal-header">
              <h3>Support Helpdesk</h3>
              <button
                type="button"
                className="support-close-btn"
                onClick={() => setIsSupportOpen(false)}
              >
                <svg
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                >
                  <line x1="18" y1="6" x2="6" y2="18" />
                  <line x1="6" y1="6" x2="18" y2="18" />
                </svg>
              </button>
            </div>

            <div className="support-info-list">
              <div className="support-info-item">
                <span className="icon">📞</span>
                <span>Phone: +1 (800) 555-DCMS (24/7 Support)</span>
              </div>
              <div className="support-info-item">
                <span className="icon">✉️</span>
                <span>Email: support@dcms-portal.com</span>
              </div>
            </div>

            {supportSent ? (
              <div className="support-success-msg">
                <div className="success-icon">✅</div>
                <h4>Message Sent!</h4>
                <p>Support team will contact you shortly.</p>
              </div>
            ) : (
              <form className="support-form" onSubmit={handleSupportSubmit}>
                <label>
                  Subject
                  <input
                    type="text"
                    required
                    placeholder="Brief summary of the issue"
                    value={supportForm.subject}
                    onChange={(e) =>
                      setSupportForm({ ...supportForm, subject: e.target.value })
                    }
                  />
                </label>
                <label>
                  Message
                  <textarea
                    required
                    rows="4"
                    placeholder="Describe your request in detail..."
                    value={supportForm.message}
                    onChange={(e) =>
                      setSupportForm({ ...supportForm, message: e.target.value })
                    }
                  />
                </label>
                <button type="submit" className="support-submit-btn">
                  Send Message
                </button>
              </form>
            )}
          </div>
        </div>
      )}
    </aside>
  );
}

export default Sidebar;
