import apiClient from './apiClient';

export async function getMyAppointments(patientId, page = 1, pageSize = 20) {
  const { data } = await apiClient.get(`/api/Appointment/by-patient/${patientId}`, {
    params: { page, pageSize },
  });
  return data;
}

export async function getUpcomingAppointments(patientId, page = 1, pageSize = 20) {
  const { data } = await apiClient.get(`/api/Appointment/upcoming/by-patient/${patientId}`, {
    params: { page, pageSize },
  });
  return data;
}

export async function getAppointmentHistory(patientId, page = 1, pageSize = 20) {
  const { data } = await apiClient.get(`/api/Appointment/history/by-patient/${patientId}`, {
    params: { page, pageSize },
  });
  return data;
}

export async function bookAppointment(dto) {
  const { data } = await apiClient.post('/api/Appointment', dto);
  return data;
}

export async function cancelAppointment(id, reason) {
  const { data } = await apiClient.put(`/api/Appointment/${id}/cancel`, { reason });
  return data;
}

export async function rescheduleAppointment(id, newDate, newStartTime) {
  const { data } = await apiClient.put(`/api/Appointment/${id}/reschedule`, {
    newDate,
    newStartTime,
  });
  return data;
}
