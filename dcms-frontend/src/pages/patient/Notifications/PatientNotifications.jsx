import { useState, useEffect, useCallback } from "react";
import "./PatientNotifications.css";

import { 
  getNotifications, 
  markNotificationAsRead, 
  markAllNotificationsAsRead 
} from "../../../services/notificationServices";

import Loading from "../../../components/common/Loading/Loading";
import EmptyState from "../../../components/common/EmptyState/EmptyState";
import Pagination from "../../../components/common/Pagination/Pagination";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";

const PAGE_SIZE = 10;

function PatientNotifications() {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  
  // Filters: 'ALL' or 'UNREAD'
  const [filter, setFilter] = useState("ALL");

  const loadNotifications = useCallback(async (page, currentFilter) => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        page: page,
        pageSize: PAGE_SIZE,
      };
      
      if (currentFilter === "UNREAD") {
        params.unreadOnly = true;
      } else {
        params.unreadOnly = false;
      }

      const response = await getNotifications(params);
      
      setNotifications(response?.items || []);
      setTotalPages(response?.totalPages || 1);
    } catch (err) {
      setError("Failed to load notifications. Please try again.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadNotifications(currentPage, filter);
  }, [currentPage, filter, loadNotifications]);

  const handleFilterChange = (val) => {
    setFilter(val);
    setCurrentPage(1); // Reset to page 1 on filter change
  };

  const handleMarkAsRead = async (id) => {
    try {
      setActionLoading(true);
      await markNotificationAsRead(id);
      // Refresh current list after marking read
      await loadNotifications(currentPage, filter);
    } catch (err) {
      console.error("Failed to mark as read", err);
      setError("Failed to mark notification as read.");
    } finally {
      setActionLoading(false);
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      setActionLoading(true);
      await markAllNotificationsAsRead();
      // Reset to page 1 and refresh
      setCurrentPage(1);
      await loadNotifications(1, filter);
    } catch (err) {
      console.error("Failed to mark all as read", err);
      setError("Failed to mark all notifications as read.");
    } finally {
      setActionLoading(false);
    }
  };

  // Determine if there are any unread notifications currently visible to enable/disable the Mark All button
  // Note: If filter="ALL", some might be unread.
  const hasUnread = notifications.some(n => !n.isRead);

  return (
    <div className="notifications-page">
      <div className="notifications-header">
        <div>
          <h1>Notifications</h1>
          <p>Stay updated with your appointments and offers.</p>
        </div>
        <div className="notifications-actions">
          <FilterDropdown
            label="Filter"
            options={[
              { value: "ALL", label: "All Notifications" },
              { value: "UNREAD", label: "Unread Only" }
            ]}
            value={filter}
            onChange={handleFilterChange}
            disabled={loading || actionLoading}
          />
          <button 
            className="mark-all-read-btn"
            onClick={handleMarkAllAsRead}
            disabled={loading || actionLoading || !hasUnread}
            title={!hasUnread ? "No unread notifications to mark" : ""}
          >
            {actionLoading ? "Processing..." : "Mark All as Read"}
          </button>
        </div>
      </div>

      {error ? (
        <EmptyState title="Error" message={error} />
      ) : loading ? (
        <Loading />
      ) : notifications.length === 0 ? (
        <EmptyState 
          title={filter === "UNREAD" ? "No unread notifications" : "No notifications"} 
          message="You're all caught up!" 
        />
      ) : (
        <>
          <div className="notifications-list">
            {notifications.map((notif) => (
              <div key={notif.id} className={`notification-card ${!notif.isRead ? "unread" : ""}`}>
                <div className="notification-content">
                  <h3>{notif.title}</h3>
                  <p>{notif.message}</p>
                  <div className="notification-meta">
                    <span>📅 {new Date(notif.createdAt).toLocaleString()}</span>
                    {/* Status Badge reused from general project css */}
                    <span className={`status-badge ${notif.isRead ? 'completed' : 'pending'}`}>
                      {notif.isRead ? "Read" : "Unread"}
                    </span>
                  </div>
                </div>
                <div className="notification-actions">
                  {!notif.isRead && (
                    <button 
                      className="mark-read-btn"
                      onClick={() => handleMarkAsRead(notif.id)}
                      disabled={actionLoading}
                    >
                      Mark as Read
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>

          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
          />
        </>
      )}
    </div>
  );
}

export default PatientNotifications;
