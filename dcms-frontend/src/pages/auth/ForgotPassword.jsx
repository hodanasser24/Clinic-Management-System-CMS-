import { useState } from "react";
import { forgotPassword } from "../../services/authServices";
import Button from "../../components/common/Button/Button";
import Input from "../../components/common/Input";
import "./Login.css";

function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();

    if (!email || !email.includes("@")) {
      setMessage("Please enter a valid email.");
      return;
    }

    setLoading(true);
    
    try {
      await forgotPassword({ email });
      setMessage("Reset token sent! Check your email.");
    } catch (err) {
      setMessage(err.message || "Failed to send reset token.");
    } finally {
      setLoading(false);
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
          {message && <div className="auth-error">{message}</div>}

          <Input
            label="Email Address"
            type="email"
            placeholder="Enter your email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />

          <Button type="submit" disabled={loading}>
            {loading ? "Sending..." : "Send Reset Token →"}
          </Button>

          <p className="auth-switch">
            Remembered your password? <a href="/login">Sign in</a>
          </p>
        </form>
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
