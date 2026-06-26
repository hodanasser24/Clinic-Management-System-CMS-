import { Link } from "react-router-dom";
import "./Navbar.css";

function Navbar() {
  return (
    <header className="navbar">
      <div className="nav-brand">
        <div className="nav-logo">🦷</div>
        <div>
          <h2>DCMS System</h2>
          <p>Dental Clinic Management System</p>
        </div>
      </div>

      <nav className="nav-links">
        <a href="#home">Home</a>
        <a href="#about">About</a>
        <a href="#services">Services</a>
        <a href="#features">Features</a>
        <a href="#contact">Contact</a>
      </nav>

      <div className="nav-actions">
        <Link className="nav-login" to="/login">
          Login
        </Link>
        <Link className="nav-register" to="/register">
          Register
        </Link>
      </div>
    </header>
  );
}

export default Navbar;
