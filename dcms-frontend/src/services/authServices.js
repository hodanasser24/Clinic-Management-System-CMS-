import apiClient from './apiClient';

export async function login(data) {
  const { data: result } = await apiClient.post('/api/Auth/login', data);
  localStorage.setItem('accessToken', result.accessToken);
  localStorage.setItem('refreshToken', result.refreshToken);
  return result;
}

export async function registerPatient(data) {
  await apiClient.post('/api/Auth/register', data);
}

export async function forgotPassword(data) {
  await apiClient.post('/api/Auth/forgot-password', data);
}

export async function resetPassword(data) {
  await apiClient.post('/api/Auth/reset-password', data);
}

export function logout() {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
}
