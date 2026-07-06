const API_BASE_URL = "http://localhost:5118/api";

async function fetchPublic(url, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || `HTTP error! status: ${response.status}`);
  }

  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

export async function getBranches() {
  return fetchPublic("/public/branches");
}

export async function getServices() {
  return fetchPublic("/public/services");
}

export async function getDoctors() {
  return fetchPublic("/public/doctors");
}

export async function getAvailableSlots(doctorId, branchId, date) {
  // date format should be YYYY-MM-DD
  return fetchPublic(`/public/doctors/${doctorId}/available-slots?branchId=${branchId}&date=${date}`);
}

export async function getActiveOffers() {
  return fetchPublic("/public/offers");
}
