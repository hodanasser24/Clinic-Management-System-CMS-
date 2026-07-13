import { 
  Star, MapPin, Phone, Mail, Clock,
  CalendarPlus, Stethoscope, ClipboardList, HeartPulse,
  ChevronDown, User
} from "lucide-react";
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
            <strong>
              <Star size={16} fill="#facc15" stroke="#facc15" />
              <Star size={16} fill="#facc15" stroke="#facc15" />
              <Star size={16} fill="#facc15" stroke="#facc15" />
              <Star size={16} fill="#facc15" stroke="#facc15" />
              <Star size={16} fill="#facc15" stroke="#facc15" />
            </strong>
            <span>Rated by 500+ patients</span>
          </div>
        </div>

        <div className="hero-visual">
          {/* Professional informational card */}
          <div className="floating-card info-card">
            <div className="info-card-icon">
              <Stethoscope size={32} />
            </div>
            <h3>Your Smile, Our Priority</h3>
            <p>
              Comprehensive dental services with state-of-the-art technology and
              a team of experienced specialists dedicated to your oral health.
            </p>
            <div className="info-card-highlights">
              <div className="info-highlight">
                <strong>15+</strong>
                <span>Services</span>
              </div>
              <div className="info-highlight">
                <strong>24/7</strong>
                <span>Support</span>
              </div>
              <div className="info-highlight">
                <strong>99%</strong>
                <span>Satisfaction</span>
              </div>
            </div>
          </div>

          {/* Patient treatment flow card */}
          <div className="dashboard-mockup treatment-flow-card">
            <div className="mockup-header">
              <h3>Your Treatment Journey</h3>
            </div>

            <div className="treatment-steps">
              <div className="treatment-step">
                <div className="step-icon">
                  <CalendarPlus size={22} />
                </div>
                <div className="step-content">
                  <strong>Book Appointment</strong>
                  <span>Schedule your visit online</span>
                </div>
              </div>

              <div className="step-connector">
                <ChevronDown size={18} />
              </div>

              <div className="treatment-step">
                <div className="step-icon">
                  <Stethoscope size={22} />
                </div>
                <div className="step-content">
                  <strong>Dental Consultation</strong>
                  <span>Expert examination & diagnosis</span>
                </div>
              </div>

              <div className="step-connector">
                <ChevronDown size={18} />
              </div>

              <div className="treatment-step">
                <div className="step-icon">
                  <ClipboardList size={22} />
                </div>
                <div className="step-content">
                  <strong>Personalized Treatment</strong>
                  <span>Tailored care plan for you</span>
                </div>
              </div>

              <div className="step-connector">
                <ChevronDown size={18} />
              </div>

              <div className="treatment-step">
                <div className="step-icon">
                  <HeartPulse size={22} />
                </div>
                <div className="step-content">
                  <strong>Continuous Follow-up</strong>
                  <span>Ongoing care & monitoring</span>
                </div>
              </div>
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
          {[1, 2, 3].map((index) => (
            <div className="doctor-card" key={index}>
              <div className="doctor-avatar">
                <User size={44} />
              </div>
              <h3>Doctor Name</h3>
              <p>Specialty</p>
              <span>Experience</span>
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
          <p><MapPin size={16} className="contact-icon" /> Cairo, Egypt</p>
          <p><Phone size={16} className="contact-icon" /> 01012345678</p>
          <p><Mail size={16} className="contact-icon" /> info@dcms.com</p>
          <p><Clock size={16} className="contact-icon" /> 09:00 AM - 09:00 PM</p>
        </div>
      </section>

      <footer className="home-footer">
        <p>&copy; 2026 DCMS. All rights reserved.</p>
      </footer>
    </main>
  );
}

export default Home;
