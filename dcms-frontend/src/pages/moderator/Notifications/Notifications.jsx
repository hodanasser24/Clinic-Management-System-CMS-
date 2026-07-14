import { useState, useEffect } from "react";
import { getNotifications, markNotificationAsRead, deleteNotification, markAllNotificationsAsRead } from "../../../services/notificationServices";
import "./Notifications.css";

function Notifications() {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchNotifications = async () => {
    try {
      setLoading(true);
      setError("");
      const result = await getNotifications({ page: 1, pageSize: 50 });
      
      // Filter out the incorrect FirstLoginMiddleware message if it somehow ended up as a normal notification
      const validNotifications = (result?.items || []).filter(
        n => n.message && !n.message.includes("You must change your temporary password before proceeding.")
      );
      
      setNotifications(validNotifications);
      setUnreadCount(result?.unreadCount || 0);
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Failed to fetch notifications.";
      setError(errorMsg);
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
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Failed to mark as read.";
      alert(errorMsg);
    }
  };

  const handleReadAll = async () => {
    try {
      await markAllNotificationsAsRead();
      setNotifications(notifications.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Failed to mark all as read.";
      alert(errorMsg);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this notification?")) return;
    try {
      await deleteNotification(id);
      const notif = notifications.find(n => n.id === id);
      if (notif && !notif.isRead) {
        setUnreadCount(prev => Math.max(0, prev - 1));
      }
      setNotifications(notifications.filter(n => n.id !== id));
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Failed to delete notification.";
      alert(errorMsg);
    }
  };

  if (loading && notifications.length === 0) return <div className="notifications-page"><div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading notifications...</div></div>;
  if (error && notifications.length === 0) return <div className="notifications-page"><p className="error">{error}</p></div>;

  return (
    <div className="notifications-page">
      <div className="page-header">
        <div>
          <h1>Notifications {unreadCount > 0 && <span style={{ fontSize: '1rem', color: '#ef476f' }}>({unreadCount} Unread)</span>}</h1>
          <p>Latest clinic notifications.</p>
        </div>
        <div style={{ display: "flex", gap: "10px" }}>
          <button onClick={fetchNotifications}>Refresh</button>
          {unreadCount > 0 && (
            <button className="primary" onClick={handleReadAll}>Mark All as Read</button>
          )}
        </div>
      </div>

      {error && <p className="error">{error}</p>}

      {notifications.length === 0 ? (
        <p>No notifications to display.</p>
      ) : (
        <div className="notification-list">
          {notifications.map((item) => (
            <div className={`notification-card ${item.isRead ? 'read' : 'unread'}`} key={item.id}>
              <h3>{item.title} {item.isRead ? <span style={{ fontSize: '0.8rem', color: '#888' }}>(Read)</span> : ""}</h3>
              <p>{item.message}</p>

              <div className="notification-footer">
                <span>{new Date(item.createdAt).toLocaleString()}</span>

                <div>
                  {!item.isRead && (
                    <button onClick={() => handleRead(item.id)}>Mark as Read</button>
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
