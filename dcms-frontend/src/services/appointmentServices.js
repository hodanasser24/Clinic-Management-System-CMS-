import apiClient from "./apiClient";

export async function getPatientAppointments(patientId, params = {}) {
  const query = new URLSearchParams();
  
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.status) query.append("status", params.status);
  if (params.doctorId) query.append("doctorId", params.doctorId);
  if (params.doctorName) query.append("doctorName", params.doctorName);
  if (params.fromDate) query.append("fromDate", params.fromDate);
  if (params.toDate) query.append("toDate", params.toDate);
  if (params.patientName) query.append("patientName", params.patientName);
  if (params.sortBy) query.append("sortBy", params.sortBy);
  if (params.sortDescending !== undefined) query.append("sortDescending", params.sortDescending);
  // Defaulting to newest first based on the requirements
  if (params.sortDescending === undefined) query.append("sortDescending", "true");

  const queryString = query.toString() ? `?${query.toString()}` : "";
  const res = await apiClient.get(`/api/Appointment/by-patient/${patientId}${queryString}`);
  return res.data;
}

export async function getDoctorAppointments(doctorId, params = {}) {
  const query = new URLSearchParams();
  
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.status !== undefined && params.status !== "") query.append("status", params.status);
  if (params.fromDate) query.append("fromDate", params.fromDate);
  if (params.toDate) query.append("toDate", params.toDate);
  if (params.patientName) query.append("patientName", params.patientName);
  if (params.sortBy) query.append("sortBy", params.sortBy);
  if (params.sortDescending !== undefined) query.append("sortDescending", params.sortDescending);
  if (params.sortDescending === undefined) query.append("sortDescending", "true");

  const queryString = query.toString() ? `?${query.toString()}` : "";
  const res = await apiClient.get(`/api/Appointment/by-doctor/${doctorId}${queryString}`);
  return res.data;
}

export async function getAppointmentById(id) {
  const res = await apiClient.get(`/api/Appointment/${id}`);
  return res.data;
}

export async function cancelAppointment(id, reason) {
  const res = await apiClient.put(`/api/Appointment/${id}/cancel`, { reason });
  return res.data;
}

export async function bookAppointment(data) {
  const res = await apiClient.post("/api/Appointment", data);
  return res.data;
}

export async function updateAppointment(id, data) {
  const res = await apiClient.put(`/api/Appointment/${id}`, data);
  return res.data;
}

export async function getUpcomingPatientAppointments(patientId, params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  const queryString = query.toString() ? `?${query.toString()}` : "";
  const res = await apiClient.get(`/api/Appointment/upcoming/by-patient/${patientId}${queryString}`);
  return res.data;
}

export async function getHistoryPatientAppointments(patientId, params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  const queryString = query.toString() ? `?${query.toString()}` : "";
  const res = await apiClient.get(`/api/Appointment/history/by-patient/${patientId}${queryString}`);
  return res.data;
}

export async function rescheduleAppointment(id, data) {
  const res = await apiClient.put(`/api/Appointment/${id}/reschedule`, data);
  return res.data;
}

export async function getAllAppointments(params = {}) {
  const query = new URLSearchParams();
  if (params.page) query.append("page", params.page);
  if (params.pageSize) query.append("pageSize", params.pageSize);
  if (params.status) query.append("status", params.status);
  if (params.id) query.append("id", params.id);
  if (params.patientName) query.append("patientName", params.patientName);
  if (params.doctorName) query.append("doctorName", params.doctorName);
  if (params.doctorId) query.append("doctorId", params.doctorId);
  if (params.fromDate) query.append("fromDate", params.fromDate);
  if (params.toDate) query.append("toDate", params.toDate);
  if (params.sortBy) query.append("sortBy", params.sortBy);
  if (params.sortDescending !== undefined) query.append("sortDescending", params.sortDescending);
  
  const queryString = query.toString() ? `?${query.toString()}` : "";
  const res = await apiClient.get(`/api/Appointment${queryString}`);
  return res.data;
}

export async function confirmAppointment(id) {
  const res = await apiClient.put(`/api/Appointment/${id}/confirm`);
  return res.data;
}

export async function rejectAppointment(id) {
  const res = await apiClient.put(`/api/Appointment/${id}/reject`);
  return res.data;
}

export async function markAttendance(id, data) {
  const res = await apiClient.put(`/api/Appointment/${id}/mark-attendance`, data);
  return res.data;
}

export async function markUrgent(id, data) {
  const res = await apiClient.put(`/api/Appointment/${id}/mark-urgent`, data);
  return res.data;
}
