import "./Profile.css";

function Profile() {
  return (
    <div className="doctor-profile-page">
      <h1>Doctor Profile</h1>

      <div className="profile-card">
        <h2>Personal Information</h2>

        <p>
          <strong>Name:</strong> Dr. Ahmed Hassan
        </p>

        <p>
          <strong>Email:</strong> doctor@dcms.com
        </p>

        <p>
          <strong>Phone:</strong> 01012345678
        </p>

        <p>
          <strong>Department:</strong> Orthodontics
        </p>

        <p>
          <strong>Experience:</strong> 8 Years
        </p>
      </div>

      <div className="profile-card">
        <h2>Biography</h2>

        <textarea rows="6" placeholder="Doctor biography..."></textarea>
      </div>

      <button className="save-profile">Save Changes</button>
    </div>
  );
}

export default Profile;
