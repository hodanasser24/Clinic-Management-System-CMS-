import { useEffect, useState } from "react";
import apiClient from "../../../services/apiClient";
import "./PrescriptionForm.css";
import { getFriendlyErrorMessage } from "../../../utils/errorMapper";

const toText = (items = []) => items.map((item) => item.medicationName).join("\n");

function PrescriptionForm({ reportId, existingPrescription, isReadOnly, onSaved }) {
  const [prescriptionText, setPrescriptionText] = useState("");
  const [instructions, setInstructions] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (existingPrescription) {
      setPrescriptionText(toText(existingPrescription.items));
      setInstructions(existingPrescription.generalInstructions || "");
    } else {
      setPrescriptionText("");
      setInstructions("");
    }
    setError("");
  }, [existingPrescription, reportId]);

  const buildItems = () =>
    prescriptionText
      .split("\n")
      .map((line) => line.trim())
      .filter(Boolean)
      .map((medicationName) => ({
        medicationName,
        dosage: "As directed",
        frequency: "As directed",
        route: "Oral",
        notes: "",
      }));

  const handleSave = async () => {
    const items = buildItems();
    if (!reportId) {
      setError("Medical report ID is missing.");
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
      let response;
      if (existingPrescription) {
        response = await apiClient.put(`/api/Prescription/${existingPrescription.id}`, payload);
      } else {
        response = await apiClient.post("/api/Prescription", payload);
      }
      
      if (onSaved) {
        onSaved(response.data);
      }
    } catch (requestError) {
      setError(getFriendlyErrorMessage(requestError));
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
      setError(getFriendlyErrorMessage(requestError));
    }
  };

  return (
    <div className="prescription-form-container">
      <h3>{existingPrescription ? "Update Prescription" : "Create Prescription"}</h3>
      <p className="prescription-subtitle">
        Prescriptions are strictly linked to this medical report.
      </p>

      {error && <p className="error">{error}</p>}

      <div className="prescription-field">
        <label>Medications</label>
        <textarea
          rows="6"
          placeholder="Write one medication per line..."
          value={prescriptionText}
          onChange={(event) => setPrescriptionText(event.target.value)}
          disabled={!reportId || isReadOnly}
        />
      </div>

      <div className="prescription-field">
        <label>Additional Instructions</label>
        <textarea
          rows="3"
          value={instructions}
          onChange={(event) => setInstructions(event.target.value)}
          disabled={!reportId || isReadOnly}
        />
      </div>

      <div className="prescription-actions">
        <button
          className="download-btn"
          onClick={handleDownload}
          disabled={!existingPrescription}
        >
          Download PDF
        </button>
        <button
          className="success"
          onClick={handleSave}
          disabled={saving || !reportId || isReadOnly}
        >
          {saving
            ? "Saving..."
            : existingPrescription
            ? "Update Prescription"
            : "Save Prescription"}
        </button>
      </div>
    </div>
  );
}

export default PrescriptionForm;
