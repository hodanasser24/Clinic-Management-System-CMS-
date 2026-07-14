import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Button from "../../components/common/Button/Button";
import Input from "../../components/common/Input";
import { forgotPassword } from "../../services/authServices";
import "./Login.css";
import { getFriendlyErrorMessage } from "../../utils/errorMapper";

function ForgotPassword() {
  const navigate = useNavigate();
  const [isDark, setIsDark] = useState(true);
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (isDark) document.body.classList.add("dark");
    else document.body.classList.remove("dark");
  }, [isDark]);

  async function handleSubmit(e) {
    e.preventDefault();
    setMessage("");
    setError("");

    if (!email || !email.includes("@")) {
      setError("Please enter a valid email.");
      return;
    }

    try {
      await forgotPassword({ email });
      setSuccess(true);
      setMessage("Reset token sent to your email!");
      setTimeout(() => {
        navigate("/reset-password");
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
            <p>Password recovery</p>
          </div>
        </div>

        <div className="auth-header">
          <h2>Forgot Password?</h2>
          <p>Enter your email and we’ll send you a reset token.</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          {error && <div className="auth-error">{error}</div>}
          {success && <div className="auth-success" style={{color: "#2ec4b6", backgroundColor: "rgba(46, 196, 182, 0.1)", padding: "0.75rem", borderRadius: "8px", marginBottom: "1rem", textAlign: "center"}}>{message}</div>}

          <Input
            label="Email Address"
            type="email"
            placeholder="Enter your email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />

          <Button type="submit">Send Reset Token →</Button>

          <p className="auth-switch">
            Remembered your password? <a href="/login">Sign in</a>
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
          <span>Secure Recovery</span>
          <h2>Recover access safely.</h2>
          <p>Your password reset process is protected and time-limited.</p>
        </div>
      </section>
    </main>
  );
}

export default ForgotPassword;
