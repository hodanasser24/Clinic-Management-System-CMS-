import "./Home.css";

function Home() {
  return (
    <main className="home-page" id="home">
      <section className="hero-section">
        <div className="hero-left">
          <div className="hero-badge">✦ Smart Dental Care</div>

          <h1>
            Manage Your Clinic. <br />
            <span>Care Your Patients.</span>
          </h1>

          <p>
            DCMS helps dental clinics manage appointments, patients, medical
            records, prescriptions and more — all in one secure platform.
          </p>

          <div className="hero-buttons">
            <a href="/login" className="primary-btn">
              Get Started →
            </a>
            <a href="#features" className="outline-btn">
              Learn More ⓘ
            </a>
          </div>

          <div className="feature-cards">
            <div className="feature-card">
              <div>📅</div>
              <h3>Easy Appointments</h3>
              <p>Schedule & manage appointments easily</p>
            </div>

            <div className="feature-card">
              <div>📁</div>
              <h3>Medical Records</h3>
              <p>Keep all patient records organized</p>
            </div>

            <div className="feature-card">
              <div>🛡</div>
              <h3>Secure & Private</h3>
              <p>Your data is safe with advanced security</p>
            </div>
          </div>
        </div>

        <div className="hero-right">
          <div className="illustration-card">
            <div className="tooth">🦷</div>
            <div className="clipboard">📋</div>
            <div className="shield">🛡</div>
          </div>
        </div>
      </section>

      <section className="stats-bar">
        <div>
          👥 <strong>500+</strong>
          <span>Happy Patients</span>
        </div>
        <div>
          🛡 <strong>99.9%</strong>
          <span>Data Security</span>
        </div>
        <div>
          🕘 <strong>24/7</strong>
          <span>Support</span>
        </div>
        <div>
          🦷 <strong>20+</strong>
          <span>Dental Specialists</span>
        </div>
      </section>
    </main>
  );
}

export default Home;
