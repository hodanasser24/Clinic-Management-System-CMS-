import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getAppointmentById, markAttendance } from "../../../services/appointmentServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import DentalChart from "../../../components/common/DentalChart/DentalChart";
import PrescriptionForm from "../../../components/common/PrescriptionForm/PrescriptionForm";
import "./AppointmentDetails.css";
import { getFriendlyErrorMessage } from "../../../utils/errorMapper";

function AppointmentDetails() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [appointment, setAppointment] = useState(null);
  
  // Medical Report State
  const [report, setReport] = useState(null);
  const [diagnosis, setDiagnosis] = useState("");
  const [treatmentPlan, setTreatmentPlan] = useState("");
  
  // Prescription State
  const [prescription, setPrescription] = useState(null);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const loadPrescription = async (reportId) => {
    try {
      const response = await apiClient.get(`/api/Prescription/by-report/${reportId}`);
      setPrescription(response.data);
    } catch (err) {
      if (err.response?.status === 404) {
        setPrescription(null); // No prescription yet
      } else {
        console.error("Failed to load prescription", err);
      }
    }
  };

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");
      
      const appointmentData = await getAppointmentById(id);
      setAppointment(appointmentData);

      const reportsResponse = await apiClient.get(`/api/Report/by-patient/${appointmentData.patientId}`, { params: { page: 1, pageSize: 50 } });
      const existingReport = (reportsResponse.data?.items || []).find((item) => String(item.appointmentId) === String(appointmentData.id)) || null;
      
      setReport(existingReport);
      setDiagnosis(existingReport?.diagnosis || "");
      setTreatmentPlan(existingReport?.treatmentPlan || existingReport?.treatment || "");

      if (existingReport) {
        await loadPrescription(existingReport.id);
      }
    } catch (requestError) {
      setError(getFriendlyErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadData(); }, [id]);

  const handleMarkAttendance = async () => {
    if (!window.confirm("Mark patient as present and complete this appointment?")) return;
    try {
      await markAttendance(id, { attendanceStatus: "Attended" });
      await loadData();
    } catch (requestError) {
      setError(getFriendlyErrorMessage(requestError));
    }
  };

  const handleSaveReport = async () => {
    if (!diagnosis.trim()) {
      setError("Diagnosis is required.");
      return;
    }
    if (appointment.status !== "Confirmed") {
      setError("Medical reports can only be saved during an active confirmed appointment.");
      return;
    }
    
    setSaving(true);
    setError("");
    const payload = { diagnosis, treatment: treatmentPlan, treatmentPlan, caseStatus: "Completed" };
    
    try {
      let savedReportId;
      if (report) {
        await apiClient.put(`/api/Report/${report.id}`, payload);
        savedReportId = report.id;
      } else {
        const response = await apiClient.post("/api/Report", { ...payload, appointmentId: appointment.id, patientId: appointment.patientId });
        savedReportId = response.data?.id;
        
        // Update local report state so PrescriptionForm becomes available without reloading
        setReport({ id: savedReportId, diagnosis, treatmentPlan });
      }
      
      // Attempt to load prescription again if it exists
      if (savedReportId) {
        await loadPrescription(savedReportId);
      }
      
      // Notice: No forced navigation here. The doctor stays on the page.
    } catch (requestError) {
      setError(getFriendlyErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  };

  const handlePrescriptionSaved = (savedPrescription) => {
    setPrescription(savedPrescription);
  };

  if (loading) return <div className="doctor-details-page"><div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading details...</div></div>;
  if (error && !appointment) return <div className="doctor-details-page"><p className="error">{error}</p></div>;
  if (!appointment) return <div className="doctor-details-page"><p>Appointment not found.</p></div>;

  const isReadOnly = appointment.status !== "Confirmed";

  const getStatusMessage = () => {
    switch (appointment.status) {
      case "Pending": return "Clinical editing will be available after the appointment is confirmed.";
      case "Completed": return "This appointment has been completed. Clinical data is now read only.";
      case "Cancelled": return "This appointment was cancelled. Clinical editing is unavailable.";
      case "Rejected": return "This appointment was rejected. Clinical editing is unavailable.";
      default: return "";
    }
  };

  const statusMessage = getStatusMessage();

  return (
    <div className="doctor-details-page">
      <div className="doctor-details-header">
        <div>
          <h1>Appointment Details</h1>
          <p>Review the visit and complete the clinical workflow.</p>
        </div>
        <button onClick={() => navigate("/doctor/appointments")}>Back</button>
      </div>

      {error && <p className="error">{error}</p>}
      {statusMessage && <div className="status-message-banner"><strong>Notice:</strong> {statusMessage}</div>}

      <div className="details-grid">
        <div className="details-card">
          <h2>Patient Information</h2>
          <p><strong>Name:</strong> {appointment.patientName}</p>
          <p><strong>Patient ID:</strong> {appointment.patientId}</p>
        </div>
        <div className="details-card">
          <h2>Appointment Information</h2>
          <p><strong>Date:</strong> {appointment.date}</p>
          <p><strong>Time:</strong> {formatTo12Hour(appointment.startTime)}</p>
          <p><strong>Service:</strong> {appointment.serviceName}</p>
          <p><strong>Branch:</strong> {appointment.branchName}</p>
          <p><strong>Status:</strong> <span className={`status-badge ${appointment.status?.toLowerCase()}`}>{appointment.status}</span></p>
          <p><strong>Attendance:</strong> {appointment.attendanceStatus}</p>
        </div>
      </div>

      <div className="details-card">
        <h2>Appointment Notes</h2>
        <p>{appointment.notes || "No notes provided."}</p>
      </div>

      <div className="details-card">
        <h2>Dental Chart</h2>
        <DentalChart 
          patientId={appointment.patientId} 
          reportId={report?.id || null} 
          appointmentId={appointment.id} 
          readOnly={isReadOnly} 
        />
      </div>

      <div className="details-card">
        <h2>Medical Report</h2>
        <div className="report-fields">
          <label>Diagnosis</label>
          <textarea 
            rows="3" 
            value={diagnosis} 
            onChange={(event) => setDiagnosis(event.target.value)} 
            placeholder="Write diagnosis here..." 
            disabled={isReadOnly} 
          />
          <label>Treatment Plan</label>
          <textarea 
            rows="3" 
            value={treatmentPlan} 
            onChange={(event) => setTreatmentPlan(event.target.value)} 
            placeholder="Write treatment plan here..." 
            disabled={isReadOnly} 
          />
        </div>
        <div className="prescription-actions">
          <button 
            className="success" 
            onClick={handleSaveReport} 
            disabled={saving || isReadOnly}
          >
            {saving ? "Saving..." : report ? "Update Report" : "Save Report"}
          </button>
        </div>
      </div>

      {report && (
        <div className="details-card">
          <PrescriptionForm 
            reportId={report.id}
            existingPrescription={prescription}
            isReadOnly={isReadOnly}
            onSaved={handlePrescriptionSaved}
          />
        </div>
      )}

      <div className="doctor-actions" style={{ justifyContent: 'center' }}>
        {appointment.status === "Confirmed" && (
          <button className="primary" onClick={handleMarkAttendance}>Complete Appointment</button>
        )}
      </div>
    </div>
  );
}

export default AppointmentDetails;
