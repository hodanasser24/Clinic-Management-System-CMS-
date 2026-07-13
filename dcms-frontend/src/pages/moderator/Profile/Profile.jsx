import { useState, useEffect } from "react";
import "./Profile.css";
import apiClient from "../../../services/apiClient";
import { getBranches } from "../../../services/publicServices";

async function getAdminProfile() {
  const res = await apiClient.get("/api/Profile/admin");
  return res.data;
}

async function updateAdminProfile(data) {
  const res = await apiClient.put("/api/Profile/admin", data);
  return res.data;
}

function Profile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({});
  const [branches, setBranches] = useState([]);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError("");
      const [data, branchData] = await Promise.all([getAdminProfile(), getBranches()]);
      setProfile(data);
      setBranches(branchData || []);
      setFormData({
        fullName: data.fullName,
        phone: data.phone || ""
      });
    } catch (err) {
      setError(err.message || "Failed to load profile");
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
      setError(err.message || "Failed to update profile");
    }
  };

  if (loading) return <div className="profile-page"><p>Loading profile...</p></div>;

  return (
    <div className="profile-page">
      <h1>Moderator Profile</h1>
      
      {error && <div style={{ color: "red", marginBottom: "1rem" }}>{error}</div>}

      <div className="profile-card">


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
            <strong>Clinic Branches</strong>
            <span>{branches.length ? branches.map((branch) => branch.name).join(", ") : "No active branches"}</span>
          </div>

          <div>
            <strong>Working Hours</strong>
            <span>{branches.length ? branches.map((branch) => `${branch.name}: ${branch.workingHours || "Not recorded"}`).join(" | ") : "Not recorded"}</span>
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
