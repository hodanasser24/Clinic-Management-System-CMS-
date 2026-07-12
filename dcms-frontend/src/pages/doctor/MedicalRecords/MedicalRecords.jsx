import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import "./MedicalRecords.css";
import DentalChart from "../../../components/common/DentalChart/DentalChart";

function MedicalRecords() {
  const navigate = useNavigate();
  const location = useLocation();
  const patientId = location.state?.patientId;

  const [patient, setPatient] = useState(null);
  const [reports, setReports] = useState([]);
  const [latestAppointmentId, setLatestAppointmentId] = useState(null);
  const [loading, setLoading] = useState(true);

  // Form states
  const [diagnosis, setDiagnosis] = useState("");
  const [treatmentPlan, setTreatmentPlan] = useState("");
  const [medications, setMedications] = useState("");
  const [doctorNotes, setDoctorNotes] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (!patientId) {
      setLoading(false);
      return;
    }

    const fetchData = async () => {
      try {
        const [patientRes, reportsRes, historyRes] = await Promise.all([
          apiClient.get(`/api/patients/${patientId}`),
          apiClient.get(`/api/patients/${patientId}/reports?page=1&pageSize=10`),
          apiClient.get(`/api/Appointment/history/by-patient/${patientId}?page=1&pageSize=1`),
        ]);
        setPatient(patientRes.data);
        setReports(reportsRes.data.items || reportsRes.data.Items || []);
        
        const historyItems = historyRes.data.items || historyRes.data.Items || [];
        if (historyItems.length > 0) {
          setLatestAppointmentId(historyItems[0].id);
        }
      } catch (err) {
        console.error("Failed to fetch patient records", err);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [patientId]);

  const calculateAge = (dob) => {
    if (!dob) return "N/A";
    const diff = Date.now() - new Date(dob).getTime();
    return Math.abs(new Date(diff).getUTCFullYear() - 1970);
  };

  const handleSaveRecord = async () => {
    if (!latestAppointmentId) {
      alert("Cannot save record: No recent appointment found for this patient.");
      return;
    }

    if (!diagnosis.trim()) {
      alert("Diagnosis is required.");
      return;
    }

    setIsSaving(true);
    try {
      const payload = {
        appointmentId: latestAppointmentId,
        patientId: patientId,
        diagnosis,
        treatmentPlan,
        treatment: medications, // Mapping medications to treatment
        internalNotes: doctorNotes,
        caseStatus: 0 // Defaulting CaseStatus or we could make it selectable
      };
      
      await apiClient.post("/api/Report", payload);
      alert("Medical record saved successfully.");
      
      // Refresh reports list
      const reportsRes = await apiClient.get(`/api/patients/${patientId}/reports?page=1&pageSize=10`);
      setReports(reportsRes.data.items || reportsRes.data.Items || []);
      
      // Clear form
      setDiagnosis("");
      setTreatmentPlan("");
      setMedications("");
      setDoctorNotes("");
    } catch (error) {
      console.error("Failed to save record:", error);
      alert("Failed to save medical record.");
    } finally {
      setIsSaving(false);
    }
  };

  if (loading) {
    return <div className="medical-page">Loading patient data...</div>;
  }

  if (!patientId) {
    return (
      <div className="medical-page">
        <div className="medical-header">
          <div>
            <h1>Medical Records</h1>
            <p>No patient selected.</p>
          </div>
          <button onClick={() => navigate("/doctor/patients")}>Go to Patients</button>
        </div>
        <div className="medical-card">
          <p>Please select a patient from the patients list to view their medical records.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="medical-page">
      <div className="medical-header">
        <div>
          <h1>Medical Records</h1>
          <p>Manage patient's diagnosis and treatment history.</p>
        </div>

        <button onClick={() => navigate("/doctor/patients")}>Back</button>
      </div>

      <div className="medical-card">
        <h2>Patient Information</h2>
        <DentalChart patientId={patientId} />

        <div className="medical-grid">
          <p>
            <strong>Name:</strong> {patient?.fullName || "N/A"}
          </p>
          <p>
            <strong>Gender:</strong> {patient?.gender || "Not specified"}
          </p>
          <p>
            <strong>Blood Type:</strong> {patient?.bloodType || "Not specified"}
          </p>
          <p>
            <strong>Allergies:</strong> {patient?.allergies || "None specified"}
          </p>
          <p>
            <strong>Chronic Diseases:</strong> {patient?.medicalHistory || "None"}
          </p>
        </div>
      </div>

      <div className="medical-card">
        <h2>Previous Reports & Medical History</h2>
        {reports.length > 0 ? (
          <table style={{ width: "100%", borderCollapse: "collapse", marginBottom: "1rem" }}>
            <thead>
              <tr style={{ textAlign: "left", borderBottom: "1px solid #ddd" }}>
                <th style={{ padding: "8px" }}>Date</th>
                <th style={{ padding: "8px" }}>Diagnosis</th>
                <th style={{ padding: "8px" }}>Treatment</th>
              </tr>
            </thead>
            <tbody>
              {reports.map((r) => (
                <tr key={r.id} style={{ borderBottom: "1px solid #eee" }}>
                  <td style={{ padding: "8px" }}>{new Date(r.createdAt).toLocaleDateString()}</td>
                  <td style={{ padding: "8px" }}>{r.diagnosis || "N/A"}</td>
                  <td style={{ padding: "8px" }}>{r.treatmentPlan || r.treatment || "N/A"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p>No previous reports found.</p>
        )}
      </div>

      <div className="medical-card">
        <h2>Diagnosis</h2>
        <textarea 
          rows="5" 
          placeholder="Write diagnosis..." 
          value={diagnosis}
          onChange={(e) => setDiagnosis(e.target.value)}
        />
      </div>

      <div className="medical-card">
        <h2>Treatment Plan</h2>
        <textarea 
          rows="5" 
          placeholder="Write treatment plan..." 
          value={treatmentPlan}
          onChange={(e) => setTreatmentPlan(e.target.value)}
        />
      </div>

      <div className="medical-card">
        <h2>Medications</h2>
        <textarea 
          rows="4" 
          placeholder="Current medications (mapped to Treatment)..." 
          value={medications}
          onChange={(e) => setMedications(e.target.value)}
        />
      </div>

      <div className="medical-card">
        <h2>Doctor Notes</h2>
        <textarea 
          rows="6" 
          placeholder="Additional internal notes..." 
          value={doctorNotes}
          onChange={(e) => setDoctorNotes(e.target.value)}
        />
      </div>

      <div className="medical-actions">
        <button onClick={() => navigate("/doctor/prescriptions", { state: { patientId } })}>
          Create Prescription
        </button>

        <button 
          className="success" 
          onClick={handleSaveRecord}
          disabled={isSaving}
        >
          {isSaving ? "Saving..." : "Save Record"}
        </button>
      </div>
    </div>
  );
}

export default MedicalRecords;
