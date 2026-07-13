import { useCallback, useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import "./Prescriptions.css";

const toText = (items = []) => items.map((item) => item.medicationName).join("\n");

function Prescriptions() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const patientId = searchParams.get("patientId");
  const requestedReportId = searchParams.get("reportId");
  const doctorId = getUserId();
  const [patients, setPatients] = useState([]);
  const [patientsTotal, setPatientsTotal] = useState(0);
  const [patientPage, setPatientPage] = useState(1);
  const [patient, setPatient] = useState(null);
  const [reports, setReports] = useState([]);
  const [prescriptions, setPrescriptions] = useState([]);
  const [reportId, setReportId] = useState("");
  const [prescriptionText, setPrescriptionText] = useState("");
  const [instructions, setInstructions] = useState("");
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

  const loadPrescriptionData = useCallback(async () => {
    try {
      const [patientResponse, reportsResponse, prescriptionsResponse] = await Promise.all([
        apiClient.get(`/api/patients/${patientId}`),
        apiClient.get(`/api/Report/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } }),
        apiClient.get(`/api/Prescription/by-patient/${patientId}`, { params: { page: 1, pageSize: 50 } }),
      ]);
      const doctorReports = reportsResponse.data?.items || [];
      setPatient(patientResponse.data);
      setReports(doctorReports);
      setPrescriptions(prescriptionsResponse.data?.items || []);
      setReportId((current) => {
        const desired = requestedReportId || current;
        return doctorReports.some((report) => String(report.id) === String(desired)) ? String(desired) : String(doctorReports[0]?.id || "");
      });
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to load prescription data.");
    } finally {
      setLoading(false);
    }
  }, [doctorId, patientId, requestedReportId]);

  useEffect(() => {
    setLoading(true);
    setError("");
    if (patientId) loadPrescriptionData();
    else loadPatients();
  }, [patientId, loadPatients, loadPrescriptionData]);

  const selectedReport = reports.find((report) => String(report.id) === String(reportId));
  const existingPrescription = prescriptions.find((prescription) => String(prescription.reportId) === String(reportId));

  useEffect(() => {
    setPrescriptionText(existingPrescription ? toText(existingPrescription.items) : "");
    setInstructions(existingPrescription?.generalInstructions || "");
  }, [existingPrescription, reportId]);

  const buildItems = () => prescriptionText.split("\n").map((line) => line.trim()).filter(Boolean).map((medicationName) => ({ medicationName, dosage: "As directed", frequency: "As directed", route: "Oral", notes: "" }));

  const handleSave = async () => {
    const items = buildItems();
    if (!reportId) {
      setError("Create a medical report before creating a prescription.");
      return;
    }
    if (!items.length) {
      setError("Enter at least one medication.");
      return;
    }
    setSaving(true);
    setError("");
    try {
      const payload = { reportId: Number(reportId), generalInstructions: instructions, items };
      if (existingPrescription) await apiClient.put(`/api/Prescription/${existingPrescription.id}`, payload);
      else await apiClient.post("/api/Prescription", payload);
      await loadPrescriptionData();
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to save prescription.");
    } finally {
      setSaving(false);
    }
  };

  const handleDownload = async () => {
    if (!existingPrescription) return;
    try {
      const response = await apiClient.get(`/api/Prescription/${existingPrescription.id}/export`, { responseType: "blob" });
      const contentDisposition = response.headers["content-disposition"] || "";
      const filename = contentDisposition.match(/filename="?([^";]+)"?/i)?.[1] || `prescription-${existingPrescription.id}.pdf`;
      const blob = new Blob([response.data], { type: response.headers["content-type"] || "application/pdf" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Unable to download prescription PDF.");
    }
  };

  if (loading) return <div className="prescription-page">Loading...</div>;
  if (!patientId) return <div className="prescription-page"><div className="prescription-header"><div><h1>Prescriptions</h1><p>Select a patient to view or create prescriptions.</p></div><button onClick={() => navigate("/doctor/patients")}>Back to Patients</button></div>{error && <p className="error">{error}</p>}<DataTable columns={[{ key: "id", label: "Patient ID" }, { key: "fullName", label: "Patient Name" }, { key: "phone", label: "Phone" }]} data={patients} actions={(row) => <div className="table-actions"><button onClick={() => navigate(`/doctor/prescriptions?patientId=${row.id}`)}>View Prescriptions</button></div>} /><Pagination currentPage={patientPage} totalPages={Math.max(Math.ceil(patientsTotal / 10), 1)} onPageChange={setPatientPage} /></div>;

  return (
    <div className="prescription-page">
      <div className="prescription-header"><div><h1>{existingPrescription ? "Update Prescription" : "Create Prescription"}</h1><p>Prescriptions are linked to a completed medical report.</p></div><button onClick={() => navigate("/doctor/prescriptions")}>Back</button></div>
      {error && <p className="error">{error}</p>}
      <div className="prescription-card"><h2>Patient</h2><p><strong>Name:</strong> {patient?.fullName}</p><p><strong>Diagnosis:</strong> {selectedReport?.diagnosis || "Select a medical report"}</p></div>
      <div className="prescription-card"><h2>Medical Report</h2>{reports.length ? <select value={reportId} onChange={(event) => setReportId(event.target.value)}>{reports.map((report) => <option key={report.id} value={report.id}>{report.createdAt?.slice(0, 10)} — {report.diagnosis}</option>)}</select> : <p>No medical reports are available for this patient.</p>}</div>
      <div className="prescription-card"><h2>Prescription</h2><textarea rows="8" placeholder="Write one medication per line..." value={prescriptionText} onChange={(event) => setPrescriptionText(event.target.value)} disabled={!reportId} /></div>
      <div className="prescription-card"><h2>Additional Instructions</h2><textarea rows="4" value={instructions} onChange={(event) => setInstructions(event.target.value)} disabled={!reportId} /></div>
      <div className="prescription-actions"><button onClick={handleDownload} disabled={!existingPrescription}>Download PDF</button><button className="success" onClick={handleSave} disabled={saving || !reportId}>{saving ? "Saving..." : existingPrescription ? "Update Prescription" : "Save Prescription"}</button></div>
    </div>
  );
}

export default Prescriptions;
