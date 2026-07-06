import "./Notifications.css";

function Notifications() {
  const notifications = [
    "New appointment assigned.",

    "Patient Ahmed uploaded X-ray.",

    "Prescription approved.",

    "Follow-up reminder for Mona Hassan.",
  ];

  return (
    <div className="notifications-page">
      <h1>Notifications</h1>

      <div className="notifications-list">
        {notifications.map((item, index) => (
          <div key={index} className="notification-card">
            {item}
          </div>
        ))}
      </div>
    </div>
  );
}

export default Notifications;
