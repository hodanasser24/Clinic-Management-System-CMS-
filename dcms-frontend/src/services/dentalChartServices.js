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

export async function getPatientDentalChart(patientId) {
  return fetchWithAuth(`/DentalChart/${patientId}`);
}

export async function updateChartNotes(patientId, appointmentId, notes) {
  return fetchWithAuth(`/DentalChart/${patientId}/notes`, {
    method: "PUT",
    body: JSON.stringify({ appointmentId, notes }),
  });
}

export async function upsertToothRecord(patientId, data) {
  // data must include appointmentId
  return fetchWithAuth(`/DentalChart/${patientId}/tooth`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export async function bulkUpsertToothRecords(patientId, appointmentId, records) {
  return fetchWithAuth(`/DentalChart/${patientId}/bulk`, {
    method: "PUT",
    body: JSON.stringify({ appointmentId, records }),
  });
}
