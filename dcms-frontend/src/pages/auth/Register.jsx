import { useEffect, useState } from "react";
import Button from "../../components/common/Button";
import Input from "../../components/common/Input";
import "./Register.css";

function Register() {
  const [isDark, setIsDark] = useState(true);
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    password: "",
    phone: "",
    dateOfBirth: "",
  });
  const [error, setError] = useState("");

  useEffect(() => {
    if (isDark) document.body.classList.add("dark");
    else document.body.classList.remove("dark");
  }, [isDark]);

  function handleChange(event) {
    const { name, value } = event.target;
    setFormData({ ...formData, [name]: value });
  }

  function handleSubmit(event) {
    event.preventDefault();
    setError("");

    if (
      !formData.fullName ||
      !formData.email ||
      !formData.password ||
      !formData.phone ||
      !formData.dateOfBirth
    ) {
      setError("Please fill all required fields.");
      return;
    }

    console.log("Ready for backend register API:", formData);
    setError("Backend API is not connected yet.");
  }

  return (
    <main className="auth-page">
      <section className="auth-card register-card">
        <div className="auth-brand">
          <div className="brand-icon">🦷</div>
          <div>
            <h1>DCMS</h1>
            <p>Create patient account</p>
          </div>
        </div>

        <div className="auth-header">
          <h2>Register Patient</h2>
          <p>Create your account to book appointments and manage records.</p>
        </div>

        <form className="register-form" onSubmit={handleSubmit}>
          {error && <div className="auth-error">{error}</div>}

          <Input
            label="Full Name"
            name="fullName"
            placeholder="Enter full name"
            value={formData.fullName}
            onChange={handleChange}
          />
          <Input
            label="Email Address"
            name="email"
            type="email"
            placeholder="Enter email"
            value={formData.email}
            onChange={handleChange}
          />
          <Input
            label="Password"
            name="password"
            type="password"
            placeholder="Enter password"
            value={formData.password}
            onChange={handleChange}
          />
          <Input
            label="Phone"
            name="phone"
            placeholder="Enter phone"
            value={formData.phone}
            onChange={handleChange}
          />
          <Input
            label="Date of Birth"
            name="dateOfBirth"
            type="date"
            value={formData.dateOfBirth}
            onChange={handleChange}
          />

          <Button type="submit">Create Account →</Button>

          <p className="auth-switch">
            Already have an account? <a href="/login">Sign in</a>
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
          <span>Patient Portal</span>
          <h2>
            Start your dental <br />
            care journey.
          </h2>
          <p>
            Register once and access appointments, prescriptions, medical
            history, and notifications from one secure place.
          </p>
        </div>
      </section>
    </main>
  );
}

export default Register;
