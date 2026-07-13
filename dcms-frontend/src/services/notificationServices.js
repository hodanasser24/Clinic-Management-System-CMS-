import apiClient from "./apiClient";

export async function getUnreadNotifications(pageSize = 20) {
  const res = await apiClient.get(`/api/Notification?unreadOnly=true&page=1&pageSize=${pageSize}`);
  return res.data;
}

export async function getNotifications(params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.unreadOnly !== undefined) query.append("unreadOnly", params.unreadOnly);
  
  const queryString = query.toString() ? `?${query.toString()}` : "";
  const res = await apiClient.get(`/api/Notification${queryString}`);
  return res.data;
}

export async function markNotificationAsRead(id) {
  const res = await apiClient.patch(`/api/Notification/${id}/read`);
  return res.data;
}

export async function markAllNotificationsAsRead() {
  const res = await apiClient.patch(`/api/Notification/read-all`);
  return res.data;
}

export async function deleteNotification(id) {
  const res = await apiClient.delete(`/api/Notification/${id}`);
  return res.data;
}
