// This file will contain all Auth API calls later.
// Login.jsx and Register.jsx should not call axios directly.

export async function login(data) {
  console.log("Login API will be connected here:", data);
}

export async function registerPatient(data) {
  console.log("Register API will be connected here:", data);
}

export async function forgotPassword(data) {
  console.log("Forgot password API will be connected here:", data);
}

export async function resetPassword(data) {
  console.log("Reset password API will be connected here:", data);
}
