const API_BASE_URL = "https://localhost:7299/api";

export async function login(data) {
  const response = await fetch(`${API_BASE_URL}/Auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || errorData?.detail || `Login failed. Status: ${response.status}`);
  }

  const result = await response.json();
  
  // Store authentication data exactly as required
  localStorage.setItem("token", result.accessToken);
  localStorage.setItem("refreshToken", result.refreshToken);
  localStorage.setItem("userId", result.userId);
  localStorage.setItem("userName", result.fullName);
  localStorage.setItem("email", result.email);
  localStorage.setItem("role", result.role); // 0 or "Patient" depending on backend serialization
  
  return result;
}

export async function registerPatient(data) {
  const response = await fetch(`${API_BASE_URL}/Auth/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || errorData?.detail || `Registration failed. Status: ${response.status}`);
  }
}

export async function forgotPassword(data) {
  const response = await fetch(`${API_BASE_URL}/Auth/forgot-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || errorData?.detail || `Failed to request password reset. Status: ${response.status}`);
  }
}

export async function resetPassword(data) {
  const response = await fetch(`${API_BASE_URL}/Auth/reset-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || errorData?.detail || `Failed to reset password. Status: ${response.status}`);
  }
}

export async function changePassword(data) {
  const token = getAuthToken();
  const response = await fetch(`${API_BASE_URL}/Auth/change-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || errorData?.detail || `Failed to change password. Status: ${response.status}`);
  }
}

export async function logout() {
  const token = getAuthToken();
  if (token) {
    try {
      await fetch(`${API_BASE_URL}/Auth/logout`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });
    } catch (err) {
      console.error("Logout API failed, continuing with local cleanup:", err);
    }
  }
  
  // Always clean up locally
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("userId");
  localStorage.removeItem("userName");
  localStorage.removeItem("email");
  localStorage.removeItem("role");
}

export function getUserId() {
  const storedId = localStorage.getItem("userId");
  return storedId ? parseInt(storedId, 10) : 1; 
}

export function setUserId(id) {
  localStorage.setItem("userId", id);
}

export function getUserName() {
  return localStorage.getItem("userName") || "User";
}

export function getAuthToken() {
  return localStorage.getItem("token") || "";
}
