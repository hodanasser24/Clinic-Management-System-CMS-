import { useState } from "react";
import Button from "../../components/common/Button";
import Input from "../../components/common/Input";
import "./Login.css";

function ResetPassword() {
  const [formData, setFormData] = useState({
    email: "",
    token: "",
    newPassword: "",
  });
  const [message, setMessage] = useState("");

  function handleChange(e) {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  }

  function handleSubmit(e) {
    e.preventDefault();

    if (!formData.email || !formData.token || !formData.newPassword) {
      setMessage("Please fill all required fields.");
      return;
    }

    setMessage("Ready for backend reset password API.");
    console.log(formData);
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
          {message && <div className="auth-error">{message}</div>}

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
