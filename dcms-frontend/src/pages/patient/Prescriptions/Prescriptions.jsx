import { useNavigate } from "react-router-dom";
import "./Prescriptions.css";

function Prescriptions() {
  const navigate = useNavigate();

  const prescriptions = [
    {
      id: 1,
      medicine: "Amoxicillin",
      dosage: "500mg",
      duration: "5 Days",
      instructions: "After meals",
      doctor: "Dr. Sara",
      date: "01 Jul 2026",
    },
    {
      id: 2,
      medicine: "Pain Relief",
      dosage: "1 Tablet",
      duration: "3 Days",
      instructions: "When needed",
      doctor: "Dr. Ahmed",
      date: "18 Jun 2026",
    },
  ];

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
        {prescriptions.map((item) => (
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

            <button onClick={() => alert("Downloading prescription PDF...")}>Download PDF</button>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Prescriptions;
