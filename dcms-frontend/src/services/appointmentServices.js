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

  // Handle empty responses (like 204 No Content or empty 200)
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

export async function getPatientAppointments(patientId, params = {}) {
  const query = new URLSearchParams();
  
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.status) query.append("status", params.status);
  if (params.sortBy) query.append("sortBy", params.sortBy);
  // Defaulting to newest first based on the requirements
  query.append("sortDescending", "true");

  const queryString = query.toString() ? `?${query.toString()}` : "";
  return fetchWithAuth(`/Appointment/by-patient/${patientId}${queryString}`);
}

export async function getDoctorAppointments(doctorId, params = {}) {
  const query = new URLSearchParams();
  
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.status !== undefined && params.status !== "") query.append("status", params.status);
  if (params.fromDate) query.append("fromDate", params.fromDate);
  if (params.toDate) query.append("toDate", params.toDate);
  if (params.sortBy) query.append("sortBy", params.sortBy);
  query.append("sortDescending", "true");

  const queryString = query.toString() ? `?${query.toString()}` : "";
  return fetchWithAuth(`/Appointment/by-doctor/${doctorId}${queryString}`);
}

export async function getAppointmentById(id) {
  return fetchWithAuth(`/Appointment/${id}`);
}

export async function cancelAppointment(id, reason) {
  return fetchWithAuth(`/Appointment/${id}/cancel`, {
    method: "PUT",
    body: JSON.stringify({ reason }),
  });
}

export async function bookAppointment(data) {
  return fetchWithAuth("/Appointment", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export async function getUpcomingPatientAppointments(patientId, params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  const queryString = query.toString() ? `?${query.toString()}` : "";
  return fetchWithAuth(`/Appointment/upcoming/by-patient/${patientId}${queryString}`);
}

export async function getHistoryPatientAppointments(patientId, params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  const queryString = query.toString() ? `?${query.toString()}` : "";
  return fetchWithAuth(`/Appointment/history/by-patient/${patientId}${queryString}`);
}

export async function rescheduleAppointment(id, data) {
  return fetchWithAuth(`/Appointment/${id}/reschedule`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function getAllAppointments(params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.status) query.append("status", params.status);
  const queryString = query.toString() ? `?${query.toString()}` : "";
  return fetchWithAuth(`/Appointment${queryString}`);
}

export async function confirmAppointment(id, data) {
  return fetchWithAuth(`/Appointment/${id}/confirm`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function rejectAppointment(id, data) {
  return fetchWithAuth(`/Appointment/${id}/reject`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function markAttendance(id, data) {
  return fetchWithAuth(`/Appointment/${id}/mark-attendance`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function markUrgent(id, data) {
  return fetchWithAuth(`/Appointment/${id}/mark-urgent`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}
