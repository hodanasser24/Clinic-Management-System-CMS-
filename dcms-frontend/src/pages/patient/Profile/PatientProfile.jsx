import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import "./PatientProfile.css";

import { getPatientProfile, updatePatientProfile } from "../../../services/profileServices";
import Input from "../../../components/common/Input";
import Loading from "../../../components/common/Loading/Loading";
import EmptyState from "../../../components/common/EmptyState/EmptyState";

function PatientProfile() {
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);
  const [feedbackMessage, setFeedbackMessage] = useState(null); // { type: "success"|"error", text: "" }

  // Form state
  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [medicalHistory, setMedicalHistory] = useState("");

  // Read-only fields
  const [email, setEmail] = useState("");
  const [createdAt, setCreatedAt] = useState("");

  // Original values for dirty detection & reset (state, not ref, to allow render-time comparison)
  const [originalValues, setOriginalValues] = useState({
    fullName: "", phone: "", dateOfBirth: "", medicalHistory: ""
  });

  const loadProfile = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const profile = await getPatientProfile();

      setFullName(profile.fullName || "");
      setPhone(profile.phone || "");
      setDateOfBirth(profile.dateOfBirth || "");
      setMedicalHistory(profile.medicalHistory || "");
      setEmail(profile.email || "");
      setCreatedAt(profile.createdAt || "");

      // Snapshot original values
      const snapshot = {
        fullName: profile.fullName || "",
        phone: profile.phone || "",
        dateOfBirth: profile.dateOfBirth || "",
        medicalHistory: profile.medicalHistory || "",
      };
      setOriginalValues(snapshot);
    } catch (err) {
      setError(err.message || "Failed to load profile.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadProfile();
  }, [loadProfile]);

  // Dirty detection
  const isDirty =
    fullName !== originalValues.fullName ||
    phone !== originalValues.phone ||
    dateOfBirth !== originalValues.dateOfBirth ||
    medicalHistory !== originalValues.medicalHistory;

  // Warn before leaving with unsaved changes
  useEffect(() => {
    const handleBeforeUnload = (e) => {
      if (isDirty) {
        e.preventDefault();
        e.returnValue = "";
      }
    };
    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [isDirty]);

  const handleReset = () => {
    setFullName(originalValues.fullName);
    setPhone(originalValues.phone);
    setDateOfBirth(originalValues.dateOfBirth);
    setMedicalHistory(originalValues.medicalHistory);
    setFeedbackMessage(null);
  };

  const handleSave = async () => {
    setFeedbackMessage(null);

    // Frontend validation
    if (!fullName.trim()) {
      setFeedbackMessage({ type: "error", text: "Full Name is required." });
      return;
    }
    if (!dateOfBirth) {
      setFeedbackMessage({ type: "error", text: "Date of Birth is required." });
      return;
    }
    if (phone && !/^[0-9+\-() ]{7,20}$/.test(phone.trim())) {
      setFeedbackMessage({ type: "error", text: "Please enter a valid phone number." });
      return;
    }

    try {
      setSaving(true);
      const payload = {
        fullName: fullName.trim(),
        phone: phone.trim() || null,
        dateOfBirth: dateOfBirth,
        medicalHistory: medicalHistory.trim() || null,
      };

      await updatePatientProfile(payload);

      // Update original values snapshot after successful save
      setOriginalValues({
        fullName: payload.fullName,
        phone: payload.phone || "",
        dateOfBirth: payload.dateOfBirth,
        medicalHistory: payload.medicalHistory || "",
      });

      setFeedbackMessage({ type: "success", text: "Profile updated successfully!" });
    } catch (err) {
      setFeedbackMessage({ type: "error", text: err.message || "Failed to update profile." });
    } finally {
      setSaving(false);
    }
  };

  const handleBack = () => {
    if (isDirty) {
      const confirmed = window.confirm("You have unsaved changes. Are you sure you want to leave?");
      if (!confirmed) return;
    }
    navigate("/patient/dashboard");
  };

  if (loading) return <div className="profile-page"><Loading /></div>;
  if (error) return <div className="profile-page"><EmptyState title="Error" message={error} /></div>;

  return (
    <div className="profile-page">
      <button className="back-btn" onClick={handleBack}>
        &larr; Back to Dashboard
      </button>

      <div className="book-appointment-header">
        <h1>My Profile</h1>
        <p>View and update your personal information.</p>
      </div>

      <div className="book-appointment-form">
        {/* Inline Feedback Message */}
        {feedbackMessage && (
          <div className={feedbackMessage.type === "success" ? "form-success" : "form-error"}>
            {feedbackMessage.text}
          </div>
        )}

        {/* Personal Information Section */}
        <h3 className="profile-section-title">Personal Information</h3>

        <div className="form-row">
          <div className="form-group">
            <Input
              label="Full Name *"
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              placeholder="Enter your full name"
              disabled={saving}
            />
          </div>
          <div className="form-group">
            <Input
              label="Email (Read Only)"
              type="email"
              value={email}
              onChange={() => {}}
              disabled
              readOnly
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <Input
              label="Phone"
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="e.g. +20 123 456 7890"
              disabled={saving}
            />
          </div>
          <div className="form-group">
            <Input
              label="Date of Birth *"
              type="date"
              value={dateOfBirth}
              onChange={(e) => setDateOfBirth(e.target.value)}
              disabled={saving}
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <Input
              label="Member Since (Read Only)"
              type="text"
              value={createdAt ? new Date(createdAt).toLocaleDateString() : "—"}
              onChange={() => {}}
              disabled
              readOnly
            />
          </div>
        </div>

        {/* Medical History Section */}
        <h3 className="profile-section-title" style={{ marginTop: "32px" }}>Medical History</h3>

        <div className="form-row">
          <div className="form-group" style={{ flex: "100%" }}>
            {medicalHistory || saving ? (
              <textarea
                placeholder="Add any relevant medical history, allergies, or conditions..."
                value={medicalHistory}
                onChange={(e) => setMedicalHistory(e.target.value)}
                disabled={saving}
                rows={5}
              />
            ) : (
              <>
                <p style={{ color: "#94a3b8", fontSize: "14px", marginBottom: "12px" }}>
                  No medical history recorded yet. Adding your medical history helps your dentist provide better care.
                </p>
                <button
                  type="button"
                  onClick={() => setMedicalHistory(" ")}
                  style={{
                    background: "none",
                    border: "1px solid rgba(148, 163, 184, 0.3)",
                    color: "#60a5fa",
                    padding: "8px 16px",
                    borderRadius: "8px",
                    cursor: "pointer",
                    fontSize: "13px"
                  }}
                >
                  + Add Medical History
                </button>
              </>
            )}
          </div>
        </div>

        {/* Form Actions */}
        <div className="form-actions">
          {isDirty && (
            <button
              type="button"
              className="reset-btn"
              onClick={handleReset}
              disabled={saving}
            >
              Reset Changes
            </button>
          )}
          <button
            className="submit-booking-btn"
            onClick={handleSave}
            disabled={saving || !isDirty}
          >
            {saving ? "Saving..." : "Save Changes"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default PatientProfile;
