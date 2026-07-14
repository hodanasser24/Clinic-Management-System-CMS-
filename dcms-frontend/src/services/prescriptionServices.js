import apiClient from "./apiClient";

export const getPatientPrescriptions = async (patientId) => {
  const response = await apiClient.get('/api/Prescription/by-patient/' + patientId);
  return response.data;
};
