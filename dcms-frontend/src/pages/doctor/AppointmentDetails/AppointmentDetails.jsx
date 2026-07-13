import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getAppointmentById, markAttendance } from "../../../services/appointmentServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import DentalChart from "../../../components/common/DentalChart/DentalChart";
import "./AppointmentDetails.css";

function AppointmentDetails() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [appointment, setAppointment] = useState(null);
  const [report, setReport] = useState(null);
  const [diagnosis, setDiagnosis] = useState("");
  const [treatmentPlan, setTreatmentPlan] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");
      const appointmentData = await getAppointmentById(id);
      const reportsResponse = await apiClient.get(`/api/Report/by-patient/${appointmentData.patientId}`, { params: { page: 1, pageSize: 50 } });
      const existingReport = (reportsResponse.data?.items || []).find((item) => String(item.appointmentId) === String(appointmentData.id)) || null;
      setAppointment(appointmentData);
      setReport(existingReport);
      setDiagnosis(existingReport?.diagnosis || "");
      setTreatmentPlan(existingReport?.treatmentPlan || existingReport?.treatment || "");
    } catch (requestError) {
      setError(requestError.response?.data?.message || requestError.message || "Failed to load appointment details.");
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
      setError(requestError.response?.data?.message || requestError.message || "Failed to complete appointment.");
    }
  };

  const handleSaveReport = async () => {
    if (!diagnosis.trim()) {
      setError("Diagnosis is required.");
      return;
    }
    if (appointment.status !== "Completed") {
      setError("Complete the appointment before saving its medical report.");
      return;
    }
    setSaving(true);
    setError("");
    const payload = { diagnosis, treatment: treatmentPlan, treatmentPlan, caseStatus: "Completed" };
    try {
      if (report) {
        await apiClient.put(`/api/Report/${report.id}`, payload);
      } else {
        const response = await apiClient.post("/api/Report", { ...payload, appointmentId: appointment.id, patientId: appointment.patientId });
        // Navigation to Prescription occurs after a successful report save.
        navigate(`/doctor/prescriptions?patientId=${appointment.patientId}&reportId=${response.data?.id || ""}`);
        return;
      }
      await loadData();
    } catch (requestError) {
      setError(requestError.response?.data?.message || requestError.message || "Failed to save medical report.");
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="doctor-details-page"><p>Loading details...</p></div>;
  if (error && !appointment) return <div className="doctor-details-page"><p className="error">{error}</p></div>;
  if (!appointment) return <div className="doctor-details-page"><p>Appointment not found.</p></div>;

  return (
    <div className="doctor-details-page">
      <div className="doctor-details-header"><div><h1>Appointment Details</h1><p>Review the visit and complete its medical report.</p></div><button onClick={() => navigate("/doctor/appointments")}>Back</button></div>
      {error && <p className="error">{error}</p>}
      <div className="details-grid">
        <div className="details-card"><h2>Patient Information</h2><p><strong>Name:</strong> {appointment.patientName}</p><p><strong>Patient ID:</strong> {appointment.patientId}</p></div>
        <div className="details-card"><h2>Appointment Information</h2><p><strong>Date:</strong> {appointment.date}</p><p><strong>Time:</strong> {formatTo12Hour(appointment.startTime)}</p><p><strong>Service:</strong> {appointment.serviceName}</p><p><strong>Branch:</strong> {appointment.branchName}</p><p><strong>Status:</strong> <span className={`status-badge ${appointment.status?.toLowerCase()}`}>{appointment.status}</span></p><p><strong>Attendance:</strong> {appointment.attendanceStatus}</p></div>
      </div>
      <div className="details-card"><h2>Appointment Notes</h2><p>{appointment.notes || "No notes provided."}</p></div>
      <div className="details-card"><h2>Dental Chart</h2><DentalChart patientId={appointment.patientId} reportId={report?.id || null} /></div>
      <div className="details-card"><h2>Diagnosis</h2><textarea rows="4" value={diagnosis} onChange={(event) => setDiagnosis(event.target.value)} placeholder="Write diagnosis here..." /></div>
      <div className="details-card"><h2>Treatment Plan</h2><textarea rows="4" value={treatmentPlan} onChange={(event) => setTreatmentPlan(event.target.value)} placeholder="Write treatment plan here..." /></div>
      <div className="doctor-actions">
        {appointment.status === "Confirmed" && <button onClick={handleMarkAttendance}>Complete Appointment</button>}
        <button className="success" onClick={handleSaveReport} disabled={saving}>{saving ? "Saving..." : report ? "Update Report" : "Save Report"}</button>
        <button onClick={() => navigate(`/doctor/medical-records?patientId=${appointment.patientId}`)}>Medical Record</button>
        <button onClick={() => {
          if (!report) {
            setError("You must save the medical report before adding a prescription.");
            return;
          }
          navigate(`/doctor/prescriptions?patientId=${appointment.patientId}&reportId=${report.id}`);
        }}>+ Add Prescription</button>
      </div>
    </div>
  );
}

export default AppointmentDetails;
