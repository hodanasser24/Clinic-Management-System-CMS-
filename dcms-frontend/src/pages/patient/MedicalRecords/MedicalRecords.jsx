import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import "./MedicalRecords.css";
import DentalChart from "../../../components/common/DentalChart/DentalChart";

function MedicalRecords() {
  const navigate = useNavigate();
  const [profile, setProfile] = useState(null);
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPatientData = async () => {
      try {
        const userId = getUserId();
        const [profileRes, historyRes] = await Promise.all([
          apiClient.get("/api/Profile/patient"),
          apiClient.get(`/api/Appointment/history/by-patient/${userId}?page=1&pageSize=50`),
        ]);

        setProfile(profileRes.data);
        setHistory(historyRes.data.items || []);
      } catch (error) {
        console.error("Failed to fetch patient medical records:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchPatientData();
  }, []);

  if (loading) {
    return <div className="patient-medical-page">Loading...</div>;
  }

  return (
    <div className="patient-medical-page">
      <div className="medical-header">
        <div>
          <h1>Medical Records</h1>
          <p>Your complete medical history.</p>
        </div>

        <button onClick={() => navigate("/patient/dashboard")}>
          Dashboard
        </button>
      </div>

      <div className="medical-card">
        <h2>Personal Information</h2>

        <div className="medical-grid">
          <p>
            <strong>Name:</strong> {profile?.fullName || "N/A"}
          </p>
          <p>
            <strong>Gender:</strong> {profile?.gender || "Not specified"}
          </p>
          <p>
            <strong>Blood Type:</strong> {profile?.bloodType || "Not specified"}
          </p>
          <p>
            <strong>Allergies:</strong> {profile?.allergies || "None specified"}
          </p>
          <p>
            <strong>Chronic Diseases:</strong> {profile?.medicalHistory || "None"}
          </p>
        </div>
      </div>

      <div className="medical-card">
        <h2>Dental Chart</h2>
        <DentalChart readOnly={true} patientId={profile?.id} />
      </div>

      <div className="medical-card">
        <h2>Medical History</h2>

        {history.length > 0 ? (
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Doctor</th>
                <th>Diagnosis</th>
                <th>Treatment</th>
              </tr>
            </thead>

            <tbody>
              {history.map((visit) => (
                <tr key={visit.id}>
                  <td>{visit.appointmentDate || visit.startTime || "N/A"}</td>
                  <td>{visit.doctorName || "N/A"}</td>
                  <td>{visit.serviceName || "N/A"}</td>
                  <td>{visit.status || "Completed"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p>No medical history found.</p>
        )}
      </div>
    </div>
  );
}

export default MedicalRecords;
