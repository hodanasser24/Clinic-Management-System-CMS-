import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import "./MedicalRecords.css";
import DentalChart from "../../../components/common/DentalChart/DentalChart";

function MedicalRecords() {
  const navigate = useNavigate();
  const [profile, setProfile] = useState(null);
  const [completedAppointments, setCompletedAppointments] = useState([]);
  const [reports, setReports] = useState([]);
  const [prescriptions, setPrescriptions] = useState([]);
  const [selectedAppointmentId, setSelectedAppointmentId] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPatientData = async () => {
      try {
        const userId = getUserId();
        const [profileRes, historyRes, reportsRes, prescriptionsRes] = await Promise.all([
          apiClient.get("/api/Profile/patient"),
          apiClient.get(`/api/Appointment/history/by-patient/${userId}?page=1&pageSize=50`),
          apiClient.get(`/api/Report/by-patient/${userId}?page=1&pageSize=50`),
          apiClient.get(`/api/Prescription/by-patient/${userId}?page=1&pageSize=50`)
        ]);

        const appointments = (historyRes.data?.items || []).filter(a => a.status === "Completed");

        setProfile(profileRes.data);
        setCompletedAppointments(appointments);
        setReports(reportsRes.data?.items || []);
        setPrescriptions(prescriptionsRes.data?.items || []);
        
        if (appointments.length > 0) {
          setSelectedAppointmentId(String(appointments[0].id));
        }
      } catch (error) {
        console.error("Failed to fetch patient medical records:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchPatientData();
  }, []);

  const selectedReport = reports.find((report) => String(report.appointmentId) === String(selectedAppointmentId));
  const selectedPrescription = prescriptions.find((prescription) => String(prescription.reportId) === String(selectedReport?.id));

  if (loading) {
    return <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading...</div>;
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
        <h2>Completed Visit</h2>
        {completedAppointments.length ? (
          <select value={selectedAppointmentId} onChange={(event) => setSelectedAppointmentId(event.target.value)}>
            {completedAppointments.map((appointment) => (
              <option key={appointment.id} value={appointment.id}>
                {appointment.date} {formatTo12Hour(appointment.startTime)} — {appointment.serviceName} (Dr. {appointment.doctorName})
              </option>
            ))}
          </select>
        ) : (
          <p>No completed visits found in your history.</p>
        )}
      </div>

      {selectedAppointmentId && <>
        <div className="medical-card"><h2>Dental Chart Snapshot</h2><DentalChart patientId={profile?.id} reportId={selectedReport?.id || null} readOnly={true} /></div>
        
        {selectedReport ? (
          <>
            <div className="medical-card"><h2>Medical Report</h2>
              <div className="readonly-report-content">
                <p><strong>Diagnosis:</strong><br/> {selectedReport.diagnosis}</p>
                {selectedReport.treatmentPlan && <p><strong>Treatment Plan:</strong><br/> {selectedReport.treatmentPlan}</p>}
                {selectedReport.treatment && <p><strong>Medications (Notes):</strong><br/> {selectedReport.treatment}</p>}
              </div>
            </div>
            
            <div className="medical-card"><h2>Prescription</h2>
              {selectedPrescription ? (
                <div className="readonly-prescription-content">
                   {selectedPrescription.generalInstructions && <p><strong>Instructions:</strong> {selectedPrescription.generalInstructions}</p>}
                   <ul>
                     {selectedPrescription.items?.map(item => (
                       <li key={item.id}><strong>{item.medicationName}</strong> — {item.dosage}, {item.frequency}, {item.route} {item.duration ? `(${item.duration})` : ""} {item.notes && <em>- {item.notes}</em>}</li>
                     ))}
                   </ul>
                </div>
              ) : (
                <p>No prescription was created for this visit.</p>
              )}
            </div>
          </>
        ) : (
          <div className="medical-card"><p>No medical report was created for this visit.</p></div>
        )}
      </>}
    </div>
  );
}

export default MedicalRecords;
