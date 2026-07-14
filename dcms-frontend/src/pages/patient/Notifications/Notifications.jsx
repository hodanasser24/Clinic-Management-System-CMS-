import { useState, useEffect } from "react";
import { getNotifications, markNotificationAsRead, deleteNotification } from "../../../services/notificationServices";
import "./Notifications.css";
import { getFriendlyErrorMessage } from "../../../utils/errorMapper";

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
      setError(getFriendlyErrorMessage(err));
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
      // Update local state to reflect change without refetching all
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
      // Remove from local state
      setNotifications(notifications.filter(n => n.id !== id));
    } catch (err) {
      alert("Failed to delete notification: " + err.message);
    }
  };

  if (loading) return <div className="patient-notifications-page"><h1>Notifications</h1><div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading notifications...</div></div>;
  if (error) return <div className="patient-notifications-page"><h1>Notifications</h1><p style={{color: "red"}}>{error}</p></div>;

  return (
    <div className="patient-notifications-page">
      <h1>Notifications</h1>

      {notifications.length === 0 ? (
        <p>No notifications to display.</p>
      ) : (
        <div className="notifications-list">
          {notifications.map((item) => (
            <div className={`notification-card ${item.isRead ? 'read' : 'unread'}`} key={item.id}>
              <h2>{item.title} {item.isRead ? "(Read)" : ""}</h2>

              <p>{item.message}</p>

              <span>{new Date(item.createdAt).toLocaleString()}</span>

              <div className="notification-actions">
                {!item.isRead && (
                  <button onClick={() => handleRead(item.id)}>Read</button>
                )}
                <button className="delete" onClick={() => handleDelete(item.id)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default Notifications;
