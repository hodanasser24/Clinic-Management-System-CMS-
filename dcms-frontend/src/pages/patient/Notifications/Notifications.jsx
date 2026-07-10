import "./Notifications.css";

function Notifications() {
  const notifications = [
    {
      id: 1,
      title: "Appointment Confirmed",
      message: "Your appointment with Dr. Sara has been confirmed.",
      date: "Today",
    },
    {
      id: 2,
      title: "Prescription Ready",
      message: "Your prescription is now available.",
      date: "Yesterday",
    },
    {
      id: 3,
      title: "Medical Record Updated",
      message: "Your medical record has been updated.",
      date: "2 Days Ago",
    },
  ];

  return (
    <div className="patient-notifications-page">
      <h1>Notifications</h1>

      <div className="notifications-list">
        {notifications.map((item) => (
          <div className="notification-card" key={item.id}>
            <h2>{item.title}</h2>

            <p>{item.message}</p>

            <span>{item.date}</span>

            <div className="notification-actions">
              <button onClick={() => alert("Notification marked as read.")}>Read</button>
              <button className="delete" onClick={() => alert("Notification deleted.")}>Delete</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Notifications;
