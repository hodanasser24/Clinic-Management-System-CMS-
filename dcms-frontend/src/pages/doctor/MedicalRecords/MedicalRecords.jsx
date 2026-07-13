import { useCallback, useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import DentalChart from "../../../components/common/DentalChart/DentalChart";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import "./MedicalRecords.css";

const emptyForm = { diagnosis: "", treatmentPlan: "", treatment: "", internalNotes: "" };

function MedicalRecords() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const patientId = searchParams.get("patientId");
  const doctorId = getUserId();
  const [patients, setPatients] = useState([]);
  const [patientsTotal, setPatientsTotal] = useState(0);
  const [patientPage, setPatientPage] = useState(1);
  const [patient, setPatient] = useState(null);
  const [completedAppointments, setCompletedAppointments] = useState([]);
  const [reports, setReports] = useState([]);
  const [selectedAppointmentId, setSelectedAppointmentId] = useState("");
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const loadPatients = useCallback(async () => {
    try {
      const response = await apiClient.get("/api/patients/search", { params: { page: patientPage, pageSize: 10 } });
      setPatients(response.data?.items || []);
      setPatientsTotal(response.data?.totalCount || 0);
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to load patients.");
    } finally {
      setLoading(false);
    }
  }, [patientPage]);

  const loadRecord = useCallback(async () => {
    try {
      const [patientResponse, reportsResponse, historyResponse] = await Promise.all([
        apiClient.get(`/api/patients/${patientId}`),
        apiClient.get(`/api/Report/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } }),
        apiClient.get(`/api/Appointment/history/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } }),
      ]);
      const appointments = (historyResponse.data?.items || []).filter(
        (appointment) => appointment.status === "Completed"
      );
      setPatient(patientResponse.data);
      setReports(reportsResponse.data?.items || []);
      setCompletedAppointments(appointments);
      setSelectedAppointmentId((current) =>
        appointments.some((appointment) => String(appointment.id) === String(current))
          ? current
          : String(appointments[0]?.id || ""),
      );
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to load medical records.");
    } finally {
      setLoading(false);
    }
  }, [doctorId, patientId]);

  useEffect(() => {
    setLoading(true);
    setError("");
    if (patientId) loadRecord();
    else loadPatients();
  }, [patientId, loadPatients, loadRecord]);

  const selectedReport = reports.find((report) => String(report.appointmentId) === String(selectedAppointmentId));

  useEffect(() => {
    if (!selectedAppointmentId) {
      setForm(emptyForm);
      return;
    }
    setForm(selectedReport
      ? {
          diagnosis: selectedReport.diagnosis || "",
          treatmentPlan: selectedReport.treatmentPlan || "",
          treatment: selectedReport.treatment || "",
          internalNotes: selectedReport.internalNotes || "",
        }
      : emptyForm);
  }, [selectedAppointmentId, selectedReport]);

  const handleSaveRecord = async () => {
    if (!selectedAppointmentId || !patientId) {
      setError("Select a completed appointment before saving a medical record.");
      return;
    }
    if (!form.diagnosis.trim()) {
      setError("Diagnosis is required.");
      return;
    }
    setSaving(true);
    setError("");
    const payload = { ...form, caseStatus: "Completed" };
    try {
      if (selectedReport) {
        await apiClient.put(`/api/Report/${selectedReport.id}`, payload);
      } else {
        await apiClient.post("/api/Report", {
          ...payload,
          appointmentId: Number(selectedAppointmentId),
          patientId: Number(patientId),
        });
      }
      await loadRecord();
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to save medical record.");
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="medical-page">Loading...</div>;

  if (!patientId) {
    return (
      <div className="medical-page">
        <div className="medical-header"><div><h1>Medical Records</h1><p>Select a patient to view their medical records.</p></div><button onClick={() => navigate("/doctor/patients")}>Back to Patients</button></div>
        {error && <p className="error">{error}</p>}
        <DataTable columns={[{ key: "id", label: "Patient ID" }, { key: "fullName", label: "Patient Name" }, { key: "phone", label: "Phone" }]} data={patients} actions={(row) => <div className="table-actions"><button onClick={() => navigate(`/doctor/medical-records?patientId=${row.id}`)}>View Records</button></div>} />
        <Pagination currentPage={patientPage} totalPages={Math.max(Math.ceil(patientsTotal / 10), 1)} onPageChange={setPatientPage} />
      </div>
    );
  }

  return (
    <div className="medical-page">
      <div className="medical-header"><div><h1>Medical Records</h1><p>Manage the completed visits assigned to you.</p></div><button onClick={() => navigate("/doctor/medical-records")}>Back</button></div>
      {error && <p className="error">{error}</p>}
      <div className="medical-card">
        <h2>Patient Information</h2>
        <div className="medical-grid"><p><strong>Name:</strong> {patient?.fullName}</p><p><strong>Gender:</strong> {patient?.gender || "Not recorded"}</p><p><strong>Blood Type:</strong> {patient?.bloodType || "Not recorded"}</p><p><strong>Allergies:</strong> {patient?.allergies || "Not recorded"}</p><p><strong>Medical History:</strong> {patient?.medicalHistory || "Not recorded"}</p></div>
      </div>
      <div className="medical-card">
        <h2>Completed Appointment</h2>
        {completedAppointments.length ? <select value={selectedAppointmentId} onChange={(event) => setSelectedAppointmentId(event.target.value)}>{completedAppointments.map((appointment) => <option key={appointment.id} value={appointment.id}>{appointment.date} {formatTo12Hour(appointment.startTime)} — {appointment.serviceName}</option>)}</select> : <p>No completed appointments are available for a medical report.</p>}
      </div>
      {selectedAppointmentId && <>
        <div className="medical-card"><h2>Dental Chart</h2><DentalChart patientId={Number(patientId)} reportId={selectedReport?.id || null} /></div>
        <div className="medical-card"><h2>Diagnosis</h2><textarea rows="5" value={form.diagnosis} onChange={(event) => setForm((current) => ({ ...current, diagnosis: event.target.value }))} /></div>
        <div className="medical-card"><h2>Treatment Plan</h2><textarea rows="5" value={form.treatmentPlan} onChange={(event) => setForm((current) => ({ ...current, treatmentPlan: event.target.value }))} /></div>
        <div className="medical-card"><h2>Medications</h2><textarea rows="4" value={form.treatment} onChange={(event) => setForm((current) => ({ ...current, treatment: event.target.value }))} /></div>
        <div className="medical-card"><h2>Doctor Notes</h2><textarea rows="6" value={form.internalNotes} onChange={(event) => setForm((current) => ({ ...current, internalNotes: event.target.value }))} /></div>
        <div className="medical-actions"><button onClick={() => navigate(`/doctor/prescriptions?patientId=${patientId}&reportId=${selectedReport?.id || ""}`)}>Create Prescription</button><button className="success" onClick={handleSaveRecord} disabled={saving}>{saving ? "Saving..." : selectedReport ? "Update Record" : "Save Record"}</button></div>
      </>}
    </div>
  );
}

export default MedicalRecords;
