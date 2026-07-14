import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Button from "../../components/common/Button/Button";
import Input from "../../components/common/Input";
import { changePassword, logout } from "../../services/authServices";
import "./Login.css";
import { getFriendlyErrorMessage } from "../../utils/errorMapper";

function ChangePassword() {
  const navigate = useNavigate();
  const [isDark, setIsDark] = useState(true);
  const [formData, setFormData] = useState({
    currentPassword: "",
    newPassword: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);
  const [message, setMessage] = useState("");

  useEffect(() => {
    if (isDark) document.body.classList.add("dark");
    else document.body.classList.remove("dark");
  }, [isDark]);

  function handleChange(e) {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setMessage("");

    if (!formData.currentPassword || !formData.newPassword) {
      setError("Please fill all required fields.");
      return;
    }

    try {
      await changePassword(formData);
      setSuccess(true);
      setMessage("Password changed successfully! Redirecting to login...");
      
      // Clear token and state so the user is forced to re-login with the new password
      await logout();
      
      setTimeout(() => {
        navigate("/login");
      }, 2000);
    } catch (err) {
      setError(getFriendlyErrorMessage(err));
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-card">
        <div className="auth-brand">
          <div className="brand-icon">🦷</div>
          <div>
            <h1>DCMS</h1>
            <p>Change your password</p>
          </div>
        </div>

        <div className="auth-header">
          <h2>Change Password</h2>
          <p>This is your first time logging in, or a password change is required.</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          {error && <div className="auth-error">{error}</div>}
          {success && <div className="auth-success" style={{color: "#2ec4b6", backgroundColor: "rgba(46, 196, 182, 0.1)", padding: "0.75rem", borderRadius: "8px", marginBottom: "1rem", textAlign: "center"}}>{message}</div>}

          <Input
            label="Current Password"
            name="currentPassword"
            type="password"
            value={formData.currentPassword}
            onChange={handleChange}
          />
          <Input
            label="New Password"
            name="newPassword"
            type="password"
            value={formData.newPassword}
            onChange={handleChange}
          />

          <Button type="submit">Change Password →</Button>
        </form>

        <button
          className="theme-toggle-btn"
          onClick={() => setIsDark((prev) => !prev)}
        >
          {isDark ? "☀️ Light Mode" : "🌙 Dark Mode"}
        </button>
      </section>

      <section className="auth-hero">
        <div className="hero-content">
          <span>Security First</span>
          <h2>Secure your account.</h2>
          <p>Update your temporary password to a secure one before accessing your dashboard.</p>
        </div>
      </section>
    </main>
  );
}

export default ChangePassword;
