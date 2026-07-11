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

export async function getPatientProfile() {
  return fetchWithAuth("/Profile/patient");
}

export async function updatePatientProfile(data) {
  return fetchWithAuth("/Profile/patient", {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function getAdminProfile() {
  return fetchWithAuth("/Profile/admin");
}

export async function updateAdminProfile(data) {
  return fetchWithAuth("/Profile/admin", {
    method: "PUT",
    body: JSON.stringify(data),
  });
}
