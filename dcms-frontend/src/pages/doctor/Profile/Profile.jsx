import { useState, useEffect } from "react";
import { getAdminProfile as getProfileFallback, updateAdminProfile as updateProfileFallback } from "../../../services/profileServices";
import "./Profile.css";

// Assuming we add these to profileServices.js:
import { getAuthToken } from "../../../services/authServices";
import { getFriendlyErrorMessage } from "../../../utils/errorMapper";

async function getDoctorProfile() {
  const token = getAuthToken();
  const res = await fetch("https://localhost:7299/api/Profile/doctor", {
    headers: { "Authorization": `Bearer ${token}` }
  });
  if (!res.ok) throw new Error("Failed to load profile");
  return res.json();
}

async function updateDoctorProfile(data) {
  const token = getAuthToken();
  const res = await fetch("https://localhost:7299/api/Profile/doctor", {
    method: "PUT",
    headers: {
      "Authorization": `Bearer ${token}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify(data)
  });
  if (!res.ok) throw new Error("Failed to update profile");
  return res.json();
}

function Profile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({});

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError("");
      const data = await getDoctorProfile();
      setProfile(data);
      setFormData({
        fullName: data.fullName,
        phone: data.phone || "",
        specialization: data.specialization || "",
        qualification: data.qualification || "",
        bio: data.bio || "",
        experienceYears: data.experienceYears || 0,
      });
    } catch (err) {
      setError(getFriendlyErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProfile();
  }, []);

  const handleSave = async () => {
    try {
      setError("");
      await updateDoctorProfile(formData);
      setIsEditing(false);
      loadProfile();
    } catch (err) {
      setError(getFriendlyErrorMessage(err));
    }
  };

  if (loading) return <div className="doctor-profile-page"><div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading profile...</div></div>;

  return (
    <div className="doctor-profile-page">
      <h1>Doctor Profile</h1>

      {error && <div style={{ color: "red", marginBottom: "1rem" }}>{error}</div>}

      <div className="profile-card">
        <h2>Personal Information</h2>

        <p>
          <strong>Name:</strong>
          {isEditing ? (
            <input
              value={formData.fullName}
              onChange={e => setFormData({ ...formData, fullName: e.target.value })}
            />
          ) : (
            ` Dr. ${profile?.fullName}`
          )}
        </p>

        <p>
          <strong>Email:</strong> {profile?.email}
        </p>

        <p>
          <strong>Phone:</strong>
          {isEditing ? (
            <input
              value={formData.phone}
              onChange={e => setFormData({ ...formData, phone: e.target.value })}
            />
          ) : (
            ` ${profile?.phone || "Not set"}`
          )}
        </p>

        <p>
          <strong>Specialization:</strong>
          {isEditing ? (
            <input
              value={formData.specialization}
              onChange={e => setFormData({ ...formData, specialization: e.target.value })}
            />
          ) : (
            ` ${profile?.specialization}`
          )}
        </p>

        <p>
          <strong>Qualification:</strong>
          {isEditing ? (
            <input
              value={formData.qualification}
              onChange={e => setFormData({ ...formData, qualification: e.target.value })}
            />
          ) : (
            ` ${profile?.qualification}`
          )}
        </p>

        <p>
          <strong>Experience:</strong>
          {isEditing ? (
            <input
              type="number"
              value={formData.experienceYears}
              onChange={e => setFormData({ ...formData, experienceYears: parseInt(e.target.value, 10) || 0 })}
            />
          ) : (
            ` ${profile?.experienceYears} Years`
          )}
        </p>
      </div>

      <div className="profile-card">
        <h2>Biography</h2>

        <textarea
          rows="6"
          placeholder="Doctor biography..."
          value={isEditing ? formData.bio : profile?.bio || ""}
          onChange={e => setFormData({ ...formData, bio: e.target.value })}
          readOnly={!isEditing}
        ></textarea>
      </div>

      {isEditing ? (
        <div style={{ display: 'flex', gap: '1rem' }}>
          <button className="save-profile" onClick={handleSave}>Save Changes</button>
          <button className="secondary-btn" onClick={() => setIsEditing(false)}>Cancel</button>
        </div>
      ) : (
        <button className="save-profile" onClick={() => setIsEditing(true)}>Edit Profile</button>
      )}
    </div>
  );
}

export default Profile;
