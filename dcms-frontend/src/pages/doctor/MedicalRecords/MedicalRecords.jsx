import { useCallback, useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import DentalChart from "../../../components/common/DentalChart/DentalChart";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import "./MedicalRecords.css";

function MedicalRecords() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const patientId = searchParams.get("patientId");
  const doctorId = getUserId();
  const [patients, setPatients] = useState([]);
  const [patientPage, setPatientPage] = useState(1);
  const [patient, setPatient] = useState(null);
  const [completedAppointments, setCompletedAppointments] = useState([]);
  const [reports, setReports] = useState([]);
  const [prescriptions, setPrescriptions] = useState([]);
  const [selectedAppointmentId, setSelectedAppointmentId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadPatients = useCallback(async () => {
    try {
      // For doctor, load only patients they have completed appointments with
      const response = await apiClient.get(`/api/Appointment/by-doctor/${doctorId}?Status=Completed&page=1&pageSize=1000`);
      const appointments = response.data?.items || [];
      const uniquePatientsMap = new Map(appointments.map(a => [a.patientId, { id: a.patientId, fullName: a.patientName }]));
      const uniquePatients = Array.from(uniquePatientsMap.values());
      setPatients(uniquePatients);
    } catch (requestError) {
      setError("Unable to load patients with completed appointments.");
    } finally {
      setLoading(false);
    }
  }, [doctorId]);

  const loadRecord = useCallback(async () => {
    try {
      const [patientResponse, reportsResponse, historyResponse, prescriptionsResponse] = await Promise.all([
        apiClient.get(`/api/patients/${patientId}`),
        apiClient.get(`/api/Report/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } }),
        apiClient.get(`/api/Appointment/history/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } }),
        apiClient.get(`/api/Prescription/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } })
      ]);
      const appointments = (historyResponse.data?.items || []).filter(
        (appointment) => appointment.status === "Completed" && appointment.doctorId === doctorId
      );
      setPatient(patientResponse.data);
      setReports(reportsResponse.data?.items || []);
      setPrescriptions(prescriptionsResponse.data?.items || []);
      setCompletedAppointments(appointments);
      setSelectedAppointmentId((current) =>
        appointments.some((appointment) => String(appointment.id) === String(current))
          ? current
          : String(appointments[0]?.id || ""),
      );
    } catch (requestError) {
      setError("Unable to load medical records.");
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
  const selectedPrescription = prescriptions.find((prescription) => String(prescription.reportId) === String(selectedReport?.id));

  if (loading) return <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading...</div>;

  if (!patientId) {
    // Client side pagination for unique patients
    const startIndex = (patientPage - 1) * 10;
    const paginatedPatients = patients.slice(startIndex, startIndex + 10);
    return (
      <div className="medical-page">
        <div className="medical-header"><div><h1>Medical Records</h1><p>Select a patient to view their medical history with you.</p></div><button onClick={() => navigate("/doctor/dashboard")}>Dashboard</button></div>
        {error && <p className="error">{error}</p>}
        <DataTable columns={[{ key: "id", label: "Patient ID" }, { key: "fullName", label: "Patient Name" }]} data={paginatedPatients} actions={(row) => <div className="table-actions"><button onClick={() => navigate(`/doctor/medical-records?patientId=${row.id}`)}>View History</button></div>} />
        <Pagination currentPage={patientPage} totalPages={Math.max(Math.ceil(patients.length / 10), 1)} onPageChange={setPatientPage} />
      </div>
    );
  }

  return (
    <div className="medical-page">
      <div className="medical-header"><div><h1>Medical History</h1><p>Review completed visits.</p></div><button onClick={() => navigate("/doctor/medical-records")}>Back</button></div>
      {error && <p className="error">{error}</p>}
      <div className="medical-card">
        <h2>Patient Information</h2>
        <div className="medical-grid"><p><strong>Name:</strong> {patient?.fullName}</p><p><strong>Gender:</strong> {patient?.gender || "Not recorded"}</p><p><strong>Blood Type:</strong> {patient?.bloodType || "Not recorded"}</p><p><strong>Allergies:</strong> {patient?.allergies || "Not recorded"}</p><p><strong>Medical History:</strong> {patient?.medicalHistory || "Not recorded"}</p></div>
      </div>
      <div className="medical-card">
        <h2>Completed Visit</h2>
        {completedAppointments.length ? <select value={selectedAppointmentId} onChange={(event) => setSelectedAppointmentId(event.target.value)}>{completedAppointments.map((appointment) => <option key={appointment.id} value={appointment.id}>{appointment.date} {formatTo12Hour(appointment.startTime)} — {appointment.serviceName}</option>)}</select> : <p>No completed visits found for this patient.</p>}
      </div>
      {selectedAppointmentId && <>
        <div className="medical-card"><h2>Dental Chart Snapshot</h2><DentalChart patientId={Number(patientId)} reportId={selectedReport?.id || null} readOnly={true} /></div>
        
        {selectedReport ? (
          <>
            <div className="medical-card"><h2>Medical Report</h2>
              <div className="readonly-report-content">
                <p><strong>Diagnosis:</strong><br/> {selectedReport.diagnosis}</p>
                {selectedReport.treatmentPlan && <p><strong>Treatment Plan:</strong><br/> {selectedReport.treatmentPlan}</p>}
                {selectedReport.treatment && <p><strong>Medications (Notes):</strong><br/> {selectedReport.treatment}</p>}
                {selectedReport.internalNotes && <p><strong>Internal Notes:</strong><br/> {selectedReport.internalNotes}</p>}
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
