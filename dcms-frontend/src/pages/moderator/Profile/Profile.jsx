import { useState, useEffect } from "react";
import "./Profile.css";
import { getAuthToken } from "../../../services/authServices";

async function getAdminProfile() {
  const token = getAuthToken();
  const res = await fetch("https://localhost:7299/api/Profile/admin", {
    headers: { "Authorization": `Bearer ${token}` }
  });
  if (!res.ok) throw new Error("Failed to load profile");
  return res.json();
}

async function updateAdminProfile(data) {
  const token = getAuthToken();
  const res = await fetch("https://localhost:7299/api/Profile/admin", {
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
      const data = await getAdminProfile();
      setProfile(data);
      setFormData({
        fullName: data.fullName,
        phone: data.phone || ""
      });
    } catch (err) {
      setError(err.message);
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
      await updateAdminProfile(formData);
      setIsEditing(false);
      loadProfile();
    } catch (err) {
      setError(err.message);
    }
  };

  if (loading) return <div className="profile-page"><p>Loading profile...</p></div>;

  return (
    <div className="profile-page">
      <h1>Moderator Profile</h1>
      
      {error && <div style={{ color: "red", marginBottom: "1rem" }}>{error}</div>}

      <div className="profile-card">
        <img src="https://placehold.co/120x120" alt="Profile" />

        <h2>
          {isEditing ? (
            <input 
              value={formData.fullName} 
              onChange={e => setFormData({...formData, fullName: e.target.value})} 
              style={{ fontSize: 'inherit', padding: '0.2rem' }}
            />
          ) : (
            profile?.fullName
          )}
        </h2>
        <p>Clinic Administrator</p>

        <div className="profile-info">
          <div>
            <strong>Email</strong>
            <span>{profile?.email}</span>
          </div>

          <div>
            <strong>Phone</strong>
            <span>
              {isEditing ? (
                <input 
                  value={formData.phone} 
                  onChange={e => setFormData({...formData, phone: e.target.value})} 
                  style={{ padding: '0.2rem' }}
                />
              ) : (
                profile?.phone || "Not set"
              )}
            </span>
          </div>

          <div>
            <strong>Branch</strong>
            <span>Main Branch (Mock)</span>
          </div>

          <div>
            <strong>Working Hours</strong>
            <span>09:00 AM - 05:00 PM (Mock)</span>
          </div>
        </div>

        {isEditing ? (
          <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
            <button onClick={handleSave}>Save Changes</button>
            <button className="secondary-btn" onClick={() => setIsEditing(false)}>Cancel</button>
          </div>
        ) : (
          <button onClick={() => setIsEditing(true)}>Edit Profile</button>
        )}
      </div>
    </div>
  );
}

export default Profile;
