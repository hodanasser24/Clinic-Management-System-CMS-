import { useState, useEffect } from "react";
import { getPatientProfile, updatePatientProfile } from "../../../services/profileServices";
import "./Profile.css";

function Profile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({});

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError("");
      const data = await getPatientProfile();
      setProfile(data);
      setFormData({
        fullName: data.fullName,
        phone: data.phone || "",
        dateOfBirth: data.dateOfBirth || "",
        medicalHistory: data.medicalHistory || "",
        bloodType: data.bloodType || "",
        gender: data.gender || "",
        allergies: data.allergies || "",
      });
    } catch (err) {
      setError(err.message || "Failed to load profile.");
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setError("");
      await updatePatientProfile(formData);
      setIsEditing(false);
      loadProfile(); // Refresh data
    } catch (err) {
      setError(err.message || "Failed to update profile.");
    }
  };

  if (loading) return <div className="patient-profile-page"><h1>My Profile</h1><p>Loading profile...</p></div>;

  return (
    <div className="patient-profile-page">
      <h1>My Profile</h1>

      {error && <div style={{ color: "red", marginBottom: "1rem" }}>{error}</div>}

      <div className="profile-card">
        <label>Full Name</label>
        <input 
          value={isEditing ? formData.fullName : profile?.fullName || ""} 
          onChange={(e) => setFormData({...formData, fullName: e.target.value})}
          readOnly={!isEditing} 
        />

        <label>Email</label>
        <input value={profile?.email || ""} readOnly />

        <label>Phone</label>
        <input 
          value={isEditing ? formData.phone : profile?.phone || ""} 
          onChange={(e) => setFormData({...formData, phone: e.target.value})}
          readOnly={!isEditing} 
        />

        <label>Date of Birth</label>
        <input 
          type="date"
          value={isEditing ? formData.dateOfBirth : profile?.dateOfBirth || ""} 
          onChange={(e) => setFormData({...formData, dateOfBirth: e.target.value})}
          readOnly={!isEditing} 
        />

        <label>Medical History</label>
        <textarea 
          value={isEditing ? formData.medicalHistory : profile?.medicalHistory || ""} 
          onChange={(e) => setFormData({...formData, medicalHistory: e.target.value})}
          readOnly={!isEditing} 
          rows="3"
        />

        <label>Gender</label>
        <input 
          value={isEditing ? formData.gender : profile?.gender || ""} 
          onChange={(e) => setFormData({...formData, gender: e.target.value})}
          readOnly={!isEditing} 
        />

        <label>Blood Type</label>
        <input 
          value={isEditing ? formData.bloodType : profile?.bloodType || ""} 
          onChange={(e) => setFormData({...formData, bloodType: e.target.value})}
          readOnly={!isEditing} 
        />

        <label>Allergies</label>
        <textarea 
          value={isEditing ? formData.allergies : profile?.allergies || ""} 
          onChange={(e) => setFormData({...formData, allergies: e.target.value})}
          readOnly={!isEditing} 
          rows="2"
        />

        {isEditing ? (
          <div style={{ display: "flex", gap: "1rem", marginTop: "1rem" }}>
            <button onClick={handleSave}>Save Changes</button>
            <button className="secondary-btn" onClick={() => {
              setIsEditing(false);
              setFormData({
                fullName: profile.fullName,
                phone: profile.phone || "",
                dateOfBirth: profile.dateOfBirth || "",
                medicalHistory: profile.medicalHistory || "",
                bloodType: profile.bloodType || "",
                gender: profile.gender || "",
                allergies: profile.allergies || "",
              });
            }}>Cancel</button>
          </div>
        ) : (
          <button onClick={() => setIsEditing(true)}>Edit Profile</button>
        )}
      </div>
    </div>
  );
}

export default Profile;
