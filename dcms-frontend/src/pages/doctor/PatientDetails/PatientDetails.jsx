import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import "./PatientDetails.css";

const formatDate = (value) =>
  value ? new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(new Date(value)) : "Not recorded";

const getAge = (dateOfBirth) => {
  if (!dateOfBirth) return "Not recorded";
  const birthDate = new Date(dateOfBirth);
  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const beforeBirthday =
    today.getMonth() < birthDate.getMonth() ||
    (today.getMonth() === birthDate.getMonth() && today.getDate() < birthDate.getDate());
  if (beforeBirthday) age -= 1;
  return age;
};

function PatientDetails() {
  const navigate = useNavigate();
  const { id: patientId } = useParams();
  const [patient, setPatient] = useState(null);
  const [reports, setReports] = useState([]);
  const [history, setHistory] = useState([]);
  const [prescriptions, setPrescriptions] = useState([]);
  const [notes, setNotes] = useState([]);
  const [noteContent, setNoteContent] = useState("");
  const [savingNote, setSavingNote] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadPatient = async () => {
      try {
        const [patientResponse, reportsResponse, historyResponse, prescriptionsResponse, notesResponse] = await Promise.all([
          apiClient.get(`/api/patients/${patientId}`),
          apiClient.get(`/api/patients/${patientId}/reports?page=1&pageSize=20`),
          apiClient.get(`/api/Appointment/history/by-patient/${patientId}?page=1&pageSize=20`),
          apiClient.get(`/api/Prescription/by-patient/${patientId}?page=1&pageSize=20`),
          apiClient.get(`/api/DoctorNote/by-patient/${patientId}`),
        ]);
        setPatient(patientResponse.data);
        setReports(reportsResponse.data?.items || []);
        setHistory(historyResponse.data?.items || []);
        setPrescriptions(prescriptionsResponse.data?.items || prescriptionsResponse.data || []);
        setNotes(notesResponse.data || []);
      } catch (requestError) {
        setError(requestError.response?.data?.message || "Unable to load this patient's details.");
      } finally {
        setLoading(false);
      }
    };

    loadPatient();
  }, [patientId]);

  const handleSaveNote = async () => {
    if (!noteContent.trim()) return;
    setSavingNote(true);
    setError("");
    try {
      const response = await apiClient.post(`/api/DoctorNote/by-patient/${patientId}`, { content: noteContent.trim() });
      setNotes((current) => [response.data, ...current]);
      setNoteContent("");
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to save doctor note.");
    } finally {
      setSavingNote(false);
    }
  };

  if (loading) return <div className="doctor-patient-details-page">Loading patient details...</div>;
  if (error || !patient) return <div className="doctor-patient-details-page">{error || "Patient not found."}</div>;

  return (
    <div className="doctor-patient-details-page">
      <div className="patient-header">
        <div>
          <h1>Patient Details</h1>
          <p>Complete medical information for this patient.</p>
        </div>
        <button onClick={() => navigate("/doctor/patients")}>Back</button>
      </div>

      <div className="patient-grid">
        <div className="patient-card">
          <h2>Personal Information</h2>
          <p><strong>Name:</strong> {patient.fullName}</p>
          <p><strong>Age:</strong> {getAge(patient.dateOfBirth)}</p>
          <p><strong>Gender:</strong> {patient.gender || "Not recorded"}</p>
          <p><strong>Phone:</strong> {patient.phone || "Not recorded"}</p>
          <p><strong>Email:</strong> {patient.email || "Not recorded"}</p>
          <p><strong>Blood Type:</strong> {patient.bloodType || "Not recorded"}</p>
        </div>

        <div className="patient-card">
          <h2>Medical History</h2>
          <p><strong>Allergies:</strong> {patient.allergies || "Not recorded"}</p>
          <p><strong>Chronic Diseases:</strong> {patient.medicalHistory || "Not recorded"}</p>
          <p><strong>Previous Visits:</strong> {history.length}</p>
          <p><strong>Last Visit:</strong> {history[0] ? formatDate(history[0].date) : "No previous visits"}</p>
          <p><strong>Prescriptions:</strong> {prescriptions.length}</p>
        </div>
      </div>

      <div className="patient-card">
        <h2>Treatment History</h2>
        {reports.length ? (
          <ul>
            {reports.map((report) => (
              <li key={report.id}>
                {report.diagnosis} {report.treatment ? `— ${report.treatment}` : ""} ({formatDate(report.createdAt)})
              </li>
            ))}
          </ul>
        ) : <p>No treatment history is available.</p>}
      </div>

      <div className="patient-card">
        <h2>Doctor Notes</h2>
        {notes.length ? <ul>{notes.map((note) => <li key={note.id}>{note.content} ({formatDate(note.createdAt)})</li>)}</ul> : <p>No doctor notes yet.</p>}
        <textarea rows="6" placeholder="Write notes..." value={noteContent} onChange={(event) => setNoteContent(event.target.value)} />
        <div className="patient-actions"><button className="success" onClick={handleSaveNote} disabled={savingNote || !noteContent.trim()}>{savingNote ? "Saving..." : "Save Notes"}</button></div>
      </div>

      <div className="patient-actions">
        <button onClick={() => navigate(`/doctor/medical-records?patientId=${patientId}`)}>Medical Record</button>
        <button onClick={() => navigate(`/doctor/prescriptions?patientId=${patientId}`)}>Prescription</button>
      </div>
    </div>
  );
}

export default PatientDetails;
