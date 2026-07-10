import "./Home.css";

function Home() {
  return (
    <main className="home-page">
      <nav className="home-navbar">
        <div className="brand">
          <div className="brand-mark">DC</div>
          <div>
            <h2>DCMS</h2>
            <span>Dental Care Management</span>
          </div>
        </div>

        <div className="nav-links">
          <a href="#services">Services</a>
          <a href="#doctors">Doctors</a>
          <a href="#about">About</a>
          <a href="#contact">Contact</a>
        </div>

        <div className="nav-auth">
          <a href="/login" className="nav-login">
            Login
          </a>

          <a href="/register" className="nav-register">
            Register
          </a>
        </div>
      </nav>

      <section className="hero-section">
        <div className="hero-content">
          <span className="hero-badge">Trusted Digital Dental Platform</span>

          <h1>
            Premium Dental Care,
            <span> Managed Smarter.</span>
          </h1>

          <p>
            DCMS connects patients, doctors, and clinic teams in one secure
            platform for appointments, medical records, prescriptions, and
            reports.
          </p>

          <div className="hero-actions">
            <a href="/login" className="primary-btn">
              Book Appointment
            </a>
            <a href="#services" className="secondary-btn">
              Explore Services
            </a>
          </div>

          <div className="hero-trust">
            <strong>★★★★★</strong>
            <span>Rated by 500+ patients</span>
          </div>
        </div>

        <div className="hero-visual">
          <div className="floating-card doctor-card-preview">
            <div className="avatar">👩‍⚕️</div>
            <h3>Dr. Sara Ahmed</h3>
            <p>Dental Surgery Specialist</p>
            <div className="mini-tags">
              <span>8 Years</span>
              <span>4.9 Rating</span>
            </div>
          </div>

          <div className="dashboard-mockup">
            <div className="mockup-header">
              <h3>Today’s Clinic</h3>
              <span>Live</span>
            </div>

            <div className="mockup-stats">
              <div>
                <strong>32</strong>
                <span>Appointments</span>
              </div>
              <div>
                <strong>24</strong>
                <span>Completed</span>
              </div>
            </div>

            <div className="appointment-preview">
              <div>
                <strong>Ahmed Ali</strong>
                <p>10:30 AM • Teeth Cleaning</p>
              </div>
              <span className="confirmed">Confirmed</span>
            </div>

            <div className="appointment-preview">
              <div>
                <strong>Mona Hassan</strong>
                <p>12:00 PM • Root Canal</p>
              </div>
              <span className="pending">Pending</span>
            </div>
          </div>
        </div>
      </section>

      <section className="stats-section">
        <div>
          <strong>500+</strong>
          <span>Happy Patients</span>
        </div>
        <div>
          <strong>20+</strong>
          <span>Specialists</span>
        </div>
        <div>
          <strong>99.9%</strong>
          <span>Data Security</span>
        </div>
        <div>
          <strong>24/7</strong>
          <span>Online Support</span>
        </div>
      </section>

      <section className="home-section" id="services">
        <div className="section-title">
          <span>Our Services</span>
          <h2>Complete Dental Care</h2>
          <p>Professional services supported by smart digital workflows.</p>
        </div>

        <div className="services-grid">
          {[
            [
              "01",
              "Teeth Cleaning",
              "Professional cleaning and oral hygiene care.",
            ],
            [
              "02",
              "Orthodontics",
              "Braces, alignment plans, and treatment tracking.",
            ],
            [
              "03",
              "Dental Surgery",
              "Safe surgical procedures with digital records.",
            ],
            [
              "04",
              "Cosmetic Dentistry",
              "Modern smile design and cosmetic treatments.",
            ],
            ["05", "Dental Implants", "Permanent solutions for missing teeth."],
            ["06", "Emergency Care", "Fast support for urgent dental cases."],
          ].map((item) => (
            <div className="service-card" key={item[0]}>
              <span>{item[0]}</span>
              <h3>{item[1]}</h3>
              <p>{item[2]}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="home-section" id="doctors">
        <div className="section-title">
          <span>Our Doctors</span>
          <h2>Meet Our Specialists</h2>
          <p>Experienced doctors delivering safe and modern dental care.</p>
        </div>

        <div className="doctors-grid">
          {[
            ["👨‍⚕️", "Dr. Ahmed Hassan", "Orthodontist", "8 Years Experience"],
            ["👩‍⚕️", "Dr. Sara Ali", "Dental Surgery", "6 Years Experience"],
            [
              "👨‍⚕️",
              "Dr. Omar Mohamed",
              "Cosmetic Dentist",
              "10 Years Experience",
            ],
          ].map((doctor) => (
            <div className="doctor-card" key={doctor[1]}>
              <div className="doctor-avatar">{doctor[0]}</div>
              <h3>{doctor[1]}</h3>
              <p>{doctor[2]}</p>
              <span>{doctor[3]}</span>
              <a href="/login">Book Appointment</a>
            </div>
          ))}
        </div>
      </section>

      <section className="about-section" id="about">
        <div>
          <span>Why DCMS?</span>
          <h2>A smarter clinic experience for everyone.</h2>
          <p>
            Patients can book appointments and view medical records, doctors can
            manage visits and prescriptions, and clinic moderators can organize
            operations from one secure platform.
          </p>
        </div>

        <div className="about-list">
          <div>Secure digital medical records</div>
          <div>Role-based dashboards</div>
          <div>Smart appointment management</div>
          <div>Reports and daily clinic overview</div>
        </div>
      </section>

      <section className="contact-section" id="contact">
        <div>
          <span>Contact Us</span>
          <h2>Ready to visit our clinic?</h2>
          <p>
            Book your appointment and let our specialists take care of your
            smile.
          </p>
        </div>

        <div className="contact-info">
          <p>📍 Cairo, Egypt</p>
          <p>📞 01012345678</p>
          <p>✉️ info@dcms.com</p>
          <p>🕘 09:00 AM - 09:00 PM</p>
        </div>
      </section>

      <footer className="home-footer">
        <p>© 2026 DCMS. All rights reserved.</p>
      </footer>
    </main>
  );
}

export default Home;
