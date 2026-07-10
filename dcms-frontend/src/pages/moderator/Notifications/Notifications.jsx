import "./Notifications.css";

function Notifications() {
  const notifications = [
    {
      id: 1,
      title: "New Appointment",
      message: "Ahmed Ali booked a new appointment.",
      time: "5 min ago",
    },
    {
      id: 2,
      title: "Appointment Cancelled",
      message: "Mona Hassan cancelled today's appointment.",
      time: "20 min ago",
    },
    {
      id: 3,
      title: "Payment Received",
      message: "A payment has been completed successfully.",
      time: "1 hour ago",
    },
  ];

  return (
    <div className="notifications-page">
      <div className="page-header">
        <div>
          <h1>Notifications</h1>
          <p>Latest clinic notifications.</p>
        </div>
      </div>

      <div className="notification-list">
        {notifications.map((item) => (
          <div className="notification-card" key={item.id}>
            <h3>{item.title}</h3>
            <p>{item.message}</p>

            <div className="notification-footer">
              <span>{item.time}</span>

              <div>
                <button onClick={() => alert("Notification marked as read.")}>Read</button>
                <button className="danger" onClick={() => alert("Notification deleted.")}>Delete</button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Notifications;
