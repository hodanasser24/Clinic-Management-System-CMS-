import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Button from "../../components/common/Button/Button";
import Input from "../../components/common/Input";
import { resetPassword } from "../../services/authServices";
import "./Login.css";

function ResetPassword() {
  const navigate = useNavigate();
  const [isDark, setIsDark] = useState(true);
  const [formData, setFormData] = useState({
    email: "",
    token: "",
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

    if (!formData.email || !formData.token || !formData.newPassword) {
      setError("Please fill all required fields.");
      return;
    }

    try {
      await resetPassword(formData);
      setSuccess(true);
      setMessage("Password reset successful! Redirecting to login...");
      setTimeout(() => {
        navigate("/login");
      }, 2000);
    } catch (err) {
      setError(err.message || "Failed to reset password.");
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-card">
        <div className="auth-brand">
          <div className="brand-icon">🦷</div>
          <div>
            <h1>DCMS</h1>
            <p>Reset your password</p>
          </div>
        </div>

        <div className="auth-header">
          <h2>Reset Password</h2>
          <p>Enter the token sent to your email and your new password.</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          {error && <div className="auth-error">{error}</div>}
          {success && <div className="auth-success" style={{color: "#2ec4b6", backgroundColor: "rgba(46, 196, 182, 0.1)", padding: "0.75rem", borderRadius: "8px", marginBottom: "1rem", textAlign: "center"}}>{message}</div>}

          <Input
            label="Email"
            name="email"
            type="email"
            value={formData.email}
            onChange={handleChange}
          />
          <Input
            label="Reset Token"
            name="token"
            value={formData.token}
            onChange={handleChange}
          />
          <Input
            label="New Password"
            name="newPassword"
            type="password"
            value={formData.newPassword}
            onChange={handleChange}
          />

          <Button type="submit">Reset Password →</Button>

          <p className="auth-switch">
            Back to <a href="/login">Login</a>
          </p>
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
          <span>New Password</span>
          <h2>Keep your account protected.</h2>
          <p>Use a strong password to secure your dental clinic account.</p>
        </div>
      </section>
    </main>
  );
}

export default ResetPassword;
