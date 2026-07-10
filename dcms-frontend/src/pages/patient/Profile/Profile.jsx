import "./Profile.css";

function Profile() {
  return (
    <div className="patient-profile-page">
      <h1>My Profile</h1>

      <div className="profile-card">
        <label>Full Name</label>
        <input value="Ahmed Ali" readOnly />

        <label>Email</label>
        <input value="ahmed@gmail.com" readOnly />

        <label>Phone</label>
        <input value="01012345678" readOnly />

        <label>Address</label>
        <input value="Cairo, Egypt" readOnly />

        <label>Blood Type</label>
        <input value="O+" readOnly />

        <button onClick={() => alert("Edit profile details.")}>Edit Profile</button>
      </div>
    </div>
  );
}

export default Profile;
