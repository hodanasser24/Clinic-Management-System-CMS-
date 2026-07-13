import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import "./PatientDetails.css";

const getAge = (dateOfBirth) => {
  if (!dateOfBirth) return "Not recorded";
  const birthDate = new Date(dateOfBirth);
  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  if (today.getMonth() < birthDate.getMonth() || (today.getMonth() === birthDate.getMonth() && today.getDate() < birthDate.getDate())) age -= 1;
  return age;
};

function PatientDetails() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [patient, setPatient] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    apiClient.get(`/api/patients/${id}`)
      .then((response) => setPatient(response.data))
      .catch((requestError) => setError(requestError.response?.data?.message || "Unable to load this patient's details."));
  }, [id]);

  if (error) return <div className="patient-details-page">{error}</div>;
  if (!patient) return <div className="patient-details-page">Loading patient details...</div>;

  return (
    <div className="patient-details-page">
      <div className="details-header">
        <div><h1>Patient Details</h1><p>View patient profile and medical information.</p></div>
        <button onClick={() => navigate("/moderator/patients")}>Back</button>
      </div>
      <div className="details-card">
        <section>
          <h2>Personal Information</h2>
          <p><strong>Name:</strong> {patient.fullName}</p>
          <p><strong>Phone:</strong> {patient.phone || "Not recorded"}</p>
          <p><strong>Email:</strong> {patient.email || "Not recorded"}</p>
          <p><strong>Gender:</strong> {patient.gender || "Not recorded"}</p>
          <p><strong>Age:</strong> {getAge(patient.dateOfBirth)}</p>
        </section>
        <section>
          <h2>Medical Information</h2>
          <p><strong>Blood Group:</strong> {patient.bloodType || "Not recorded"}</p>
          <p><strong>Allergies:</strong> {patient.allergies || "Not recorded"}</p>
          <p><strong>Chronic Diseases:</strong> {patient.medicalHistory || "Not recorded"}</p>
        </section>
      </div>
    </div>
  );
}

export default PatientDetails;
