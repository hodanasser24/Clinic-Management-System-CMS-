import { useEffect, useState } from "react";
import LoginForm from "../../components/forms/LoginForm";
import "./Login.css";

function Login() {
  const [isDark, setIsDark] = useState(true);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isDark) document.body.classList.add("dark");
    else document.body.classList.remove("dark");
  }, [isDark]);

  function handleLogin(event) {
    event.preventDefault();
    setError("");

    if (!email || !password) {
      setError("Please enter email and password.");
      return;
    }

    if (!email.includes("@")) {
      setError("Please enter a valid email address.");
      return;
    }

    setLoading(true);

    setTimeout(() => {
      setLoading(false);
      setError("Backend API is not connected yet.");
      console.log("Ready for backend:", { email, password });
    }, 700);
  }

  return (
    <main className="auth-page">
      <section className="auth-card">
        <div className="auth-brand">
          <div className="brand-icon">🦷</div>
          <div>
            <h1>DCMS</h1>
            <p>Dental Clinic Management System</p>
          </div>
        </div>

        <div className="auth-header">
          <h2>Welcome back</h2>
          <p>Sign in to manage appointments, records, and dental care.</p>
        </div>

        <LoginForm
          email={email}
          password={password}
          onEmailChange={(e) => setEmail(e.target.value)}
          onPasswordChange={(e) => setPassword(e.target.value)}
          onSubmit={handleLogin}
          loading={loading}
          error={error}
        />

        <button
          className="theme-toggle-btn"
          onClick={() => setIsDark((prev) => !prev)}
        >
          {isDark ? "☀️ Light Mode" : "🌙 Dark Mode"}
        </button>
      </section>

      <section className="auth-hero">
        <div className="hero-content">
          <span>Smart Dental Care</span>
          <h2>
            Healthy smiles, <br />
            managed smarter.
          </h2>
          <p>
            One secure platform for appointments, medical records,
            prescriptions, and clinic operations.
          </p>

          <div className="hero-features">
            <div>📅 Easy Appointments</div>
            <div>📁 Medical Records</div>
            <div>🛡 Secure Access</div>
          </div>
        </div>
      </section>
    </main>
  );
}

export default Login;
