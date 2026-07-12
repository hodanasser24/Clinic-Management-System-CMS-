import apiClient from "./apiClient";

export async function getAllAdmins(page = 1, pageSize = 20) {
  const response = await apiClient.get(`/api/Owner/admins?page=${page}&pageSize=${pageSize}`);
  return response.data;
}

export async function createAdmin(data) {
  const response = await apiClient.post("/api/Owner/admins", data);
  return response.data;
}

export async function getAllDoctors(page = 1, pageSize = 20) {
  const response = await apiClient.get(`/api/Owner/doctors?page=${page}&pageSize=${pageSize}`);
  return response.data;
}

export async function createDoctor(data) {
  const response = await apiClient.post("/api/Owner/doctors", data);
  return response.data;
}

export async function deactivateAccount(userId, reason) {
  const response = await apiClient.put(`/api/Owner/accounts/${userId}/deactivate`, { reason });
  return response.data;
}

export async function reactivateAccount(userId) {
  const response = await apiClient.put(`/api/Owner/accounts/${userId}/reactivate`);
  return response.data;
}
