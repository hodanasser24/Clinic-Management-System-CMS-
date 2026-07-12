import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import "./Prescriptions.css";

function Prescriptions() {
  const navigate = useNavigate();
  const location = useLocation();
  const patientId = location.state?.patientId;
  const passedReportId = location.state?.reportId;

  const [patient, setPatient] = useState(null);
  const [reportId, setReportId] = useState(passedReportId || null);
  const [prescriptionText, setPrescriptionText] = useState("");
  const [additionalInstructions, setAdditionalInstructions] = useState("");
  const [loading, setLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [savedRxId, setSavedRxId] = useState(null);

  useEffect(() => {
    if (!patientId) {
      setLoading(false);
      return;
    }

    const fetchData = async () => {
      try {
        const [patientRes, reportsRes] = await Promise.all([
          apiClient.get(`/api/patients/${patientId}`),
          !passedReportId ? apiClient.get(`/api/patients/${patientId}/reports?page=1&pageSize=1`) : Promise.resolve({ data: null })
        ]);
        
        setPatient(patientRes.data);
        
        if (!passedReportId && reportsRes.data) {
          const items = reportsRes.data.items || reportsRes.data.Items || [];
          if (items.length > 0) {
            setReportId(items[0].id);
          }
        }
      } catch (err) {
        console.error("Failed to fetch patient data", err);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [patientId, passedReportId]);

  const calculateAge = (dob) => {
    if (!dob) return "N/A";
    const diff = Date.now() - new Date(dob).getTime();
    return Math.abs(new Date(diff).getUTCFullYear() - 1970);
  };

  const handleSave = async () => {
    if (!reportId) {
      alert("Cannot create prescription without an associated medical report. Please create a medical record first.");
      return;
    }
    if (!prescriptionText.trim()) {
      alert("Please enter prescription details.");
      return;
    }

    setIsSaving(true);
    try {
      // Split the text area by lines to create basic items
      const items = prescriptionText
        .split('\n')
        .filter(line => line.trim() !== '')
        .map(line => ({
          medicationName: line.trim(),
          dosage: "As directed",
          frequency: "As directed",
          route: 0,
          notes: ""
        }));

      if (items.length === 0) {
        items.push({ medicationName: "See instructions", dosage: "N/A", route: 0 });
      }

      const payload = {
        reportId: reportId,
        generalInstructions: additionalInstructions,
        items: items
      };

      const res = await apiClient.post("/api/Prescription", payload);
      setSavedRxId(res.data.id);
      alert("Prescription saved successfully.");
    } catch (err) {
      console.error(err);
      alert("Failed to save prescription.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleDownload = async () => {
    if (!savedRxId) {
      alert("Please save the prescription first before downloading.");
      return;
    }
    try {
      const response = await apiClient.get(`/api/Prescription/${savedRxId}/export`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `prescription-${savedRxId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.parentNode.removeChild(link);
    } catch (err) {
      alert("Failed to download prescription.");
    }
  };

  if (loading) {
    return <div className="prescription-page">Loading patient data...</div>;
  }

  if (!patientId) {
    return (
      <div className="prescription-page">
        <div className="prescription-header">
          <div>
            <h1>Create Prescription</h1>
            <p>No patient selected.</p>
          </div>
          <button onClick={() => navigate("/doctor/patients")}>Go to Patients</button>
        </div>
      </div>
    );
  }

  return (
    <div className="prescription-page">
      <div className="prescription-header">
        <div>
          <h1>Create Prescription</h1>
          <p>Generate a prescription for the current patient.</p>
        </div>

        <button onClick={() => navigate("/doctor/patients")}>Back</button>
      </div>

      <div className="prescription-card">
        <h2>Patient</h2>

        <p>
          <strong>Name:</strong> {patient?.fullName || "N/A"}
        </p>
        <p>
          <strong>Age:</strong> {calculateAge(patient?.dateOfBirth)}
        </p>
        <p>
          <strong>Diagnosis:</strong> View in Medical Records
        </p>
      </div>

      <div className="prescription-card">
        <h2>Prescription</h2>

        <textarea
          rows="8"
          placeholder="Write medicines, dosage and instructions (one per line)..."
          value={prescriptionText}
          onChange={(e) => setPrescriptionText(e.target.value)}
        />
      </div>

      <div className="prescription-card">
        <h2>Additional Instructions</h2>

        <textarea 
          rows="4" 
          placeholder="Extra recommendations..."
          value={additionalInstructions}
          onChange={(e) => setAdditionalInstructions(e.target.value)}
        />
      </div>

      <div className="prescription-actions">
        <button onClick={() => window.print()}>Print</button>

        <button onClick={handleDownload} disabled={!savedRxId}>Download PDF</button>

        <button 
          className="success" 
          onClick={handleSave}
          disabled={isSaving}
        >
          {isSaving ? "Saving..." : "Save Prescription"}
        </button>
      </div>
    </div>
  );
}

export default Prescriptions;
