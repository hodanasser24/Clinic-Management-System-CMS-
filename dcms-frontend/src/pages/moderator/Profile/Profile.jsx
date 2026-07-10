import "./Profile.css";

function Profile() {
  return (
    <div className="profile-page">
      <h1>Moderator Profile</h1>

      <div className="profile-card">
        <img src="https://placehold.co/120x120" alt="Profile" />

        <h2>Moderator Name</h2>
        <p>Clinic Administrator</p>

        <div className="profile-info">
          <div>
            <strong>Email</strong>
            <span>moderator@clinic.com</span>
          </div>

          <div>
            <strong>Phone</strong>
            <span>01012345678</span>
          </div>

          <div>
            <strong>Branch</strong>
            <span>Main Branch</span>
          </div>

          <div>
            <strong>Working Hours</strong>
            <span>09:00 AM - 05:00 PM</span>
          </div>
        </div>

        <button onClick={() => alert("Edit profile dialog opened.")}>Edit Profile</button>
      </div>
    </div>
  );
}

export default Profile;
