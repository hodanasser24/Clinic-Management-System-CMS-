import { useState, useEffect } from "react";
import { getNotifications, markNotificationAsRead, deleteNotification, markAllNotificationsAsRead } from "../../../services/notificationServices";
import "./Notifications.css";

function Notifications() {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchNotifications = async () => {
    try {
      setLoading(true);
      setError("");
      const result = await getNotifications({ page: 1, pageSize: 50 });
      setNotifications(result?.items || []);
    } catch (err) {
      setError(err.message || "Failed to fetch notifications.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNotifications();
  }, []);

  const handleRead = async (id) => {
    try {
      await markNotificationAsRead(id);
      setNotifications(notifications.map(n => 
        n.id === id ? { ...n, isRead: true } : n
      ));
    } catch (err) {
      alert("Failed to mark as read: " + err.message);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this notification?")) return;
    try {
      await deleteNotification(id);
      setNotifications(notifications.filter(n => n.id !== id));
    } catch (err) {
      alert("Failed to delete notification: " + err.message);
    }
  };

  if (loading) return <div className="notifications-page"><p>Loading notifications...</p></div>;
  if (error) return <div className="notifications-page"><p className="error">{error}</p></div>;

  return (
    <div className="notifications-page">
      <div className="page-header">
        <div>
          <h1>Notifications</h1>
          <p>Latest clinic notifications.</p>
        </div>
      </div>

      {notifications.length === 0 ? (
        <p>No notifications to display.</p>
      ) : (
        <div className="notification-list">
          {notifications.map((item) => (
            <div className={`notification-card ${item.isRead ? 'read' : 'unread'}`} key={item.id}>
              <h3>{item.title} {item.isRead ? "(Read)" : ""}</h3>
              <p>{item.message}</p>

              <div className="notification-footer">
                <span>{new Date(item.createdAt).toLocaleString()}</span>

                <div>
                  {!item.isRead && (
                    <button onClick={() => handleRead(item.id)}>Read</button>
                  )}
                  <button className="danger" onClick={() => handleDelete(item.id)}>Delete</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default Notifications;
