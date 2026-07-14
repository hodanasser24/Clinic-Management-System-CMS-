import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import "./Prescriptions.css";

function Prescriptions() {
  const navigate = useNavigate();
  const [prescriptions, setPrescriptions] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPrescriptions = async () => {
      try {
        const userId = getUserId();
        
        const res = await apiClient.get(`/api/Prescription/by-patient/${userId}?page=1&pageSize=50`);
        const pagedItems = res.data.items || res.data.Items || [];

        const allPrescriptions = [];
        pagedItems.forEach(rx => {
          if (rx.items && rx.items.length > 0) {
            rx.items.forEach(item => {
              allPrescriptions.push({
                id: item.id,
                rxId: rx.id,
                medicine: item.medicationName,
                dosage: item.dosage,
                duration: item.duration || "N/A",
                instructions: item.notes || item.frequency || rx.generalInstructions || "N/A",
                doctor: `Doctor ID: ${rx.doctorId}`, // In a real app we'd join the doctor name, this handles the current structure
                date: new Date(rx.createdAt).toLocaleDateString()
              });
            });
          }
        });
        
        setPrescriptions(allPrescriptions);
      } catch (error) {
        console.error("Failed to fetch patient data", error);
      } finally {
        setLoading(false);
      }
    };

    fetchPrescriptions();
  }, []);

  const handleDownload = async (rxId) => {
    try {
      const response = await apiClient.get(`/api/Prescription/${rxId}/export`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `prescription-${rxId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.parentNode.removeChild(link);
    } catch (err) {
      alert("Failed to download prescription.");
    }
  };

  return (
    <div className="patient-prescriptions-page">
      <div className="prescriptions-header">
        <div>
          <h1>Prescriptions</h1>
          <p>View your medicines and doctor instructions.</p>
        </div>

        <button onClick={() => navigate("/patient/dashboard")}>
          Dashboard
        </button>
      </div>

      <div className="prescription-list">
        {loading ? (
          <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading prescriptions...</div>
        ) : prescriptions.length > 0 ? (
          prescriptions.map((item) => (
            <div className="prescription-card" key={item.id}>
              <h2>{item.medicine}</h2>

              <p>
                <strong>Dosage:</strong> {item.dosage}
              </p>
              <p>
                <strong>Duration:</strong> {item.duration}
              </p>
              <p>
                <strong>Instructions:</strong> {item.instructions}
              </p>
              <p>
                <strong>Doctor:</strong> {item.doctor}
              </p>
              <p>
                <strong>Date:</strong> {item.date}
              </p>

              <button onClick={() => handleDownload(item.rxId)}>Download PDF</button>
            </div>
          ))
        ) : (
          <p>No prescriptions found.</p>
        )}
      </div>
    </div>
  );
}

export default Prescriptions;
