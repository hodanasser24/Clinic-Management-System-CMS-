import { getAuthToken } from "./authServices";

const API_BASE_URL = "https://localhost:7299/api";

async function fetchWithAuth(url, options = {}) {
  const token = getAuthToken();
  const headers = {
    "Content-Type": "application/json",
    ...options.headers,
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || errorData?.detail || `HTTP error! status: ${response.status}`);
  }

  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

export async function getUnreadNotifications(pageSize = 20) {
  return fetchWithAuth(`/Notification?unreadOnly=true&page=1&pageSize=${pageSize}`);
}

export async function getNotifications(params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.unreadOnly !== undefined) query.append("unreadOnly", params.unreadOnly);
  
  const queryString = query.toString() ? `?${query.toString()}` : "";
  return fetchWithAuth(`/Notification${queryString}`);
}

export async function markNotificationAsRead(id) {
  return fetchWithAuth(`/Notification/${id}/read`, {
    method: "PATCH",
  });
}

export async function markAllNotificationsAsRead() {
  return fetchWithAuth(`/Notification/read-all`, {
    method: "PATCH",
  });
}

export async function deleteNotification(id) {
  return fetchWithAuth(`/Notification/${id}`, {
    method: "DELETE",
  });
}
