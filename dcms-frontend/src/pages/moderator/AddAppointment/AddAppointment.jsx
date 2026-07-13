import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../../services/apiClient";
import { bookAppointment } from "../../../services/appointmentServices";
import { getBranches, getDoctors, getServices, getAvailableDates, getAvailableSlots } from "../../../services/publicServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import "./AddAppointment.css";

function AddAppointment() {
  const navigate = useNavigate();
  const [patients, setPatients] = useState([]);
  const [branches, setBranches] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [services, setServices] = useState([]);
  const [patientSearch, setPatientSearch] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ patientId: "", doctorId: "", serviceId: "", branchId: "", date: "", startTime: "", notes: "" });
  const [availableDates, setAvailableDates] = useState([]);
  const [loadingDates, setLoadingDates] = useState(false);
  const [availableSlots, setAvailableSlots] = useState([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  useEffect(() => {
    Promise.all([getBranches(), getDoctors(), getServices()])
      .then(([branchData, doctorData, serviceData]) => {
        setBranches(branchData || []);
        setDoctors(doctorData || []);
        setServices(serviceData || []);
      })
      .catch(() => setError("Unable to load appointment options."));
  }, []);

  useEffect(() => {
    if (patientSearch.trim().length < 2) {
      setPatients([]);
      return;
    }
    const timer = setTimeout(async () => {
      try {
        const response = await apiClient.get("/api/patients/search", { params: { fullName: patientSearch, page: 1, pageSize: 20 } });
        setPatients(response.data?.items || []);
      } catch {
        setError("Unable to search for patients.");
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [patientSearch]);

  useEffect(() => {
    const { doctorId, branchId } = form;
    if (doctorId && branchId) {
      setLoadingDates(true);
      getAvailableDates(doctorId, branchId)
        .then((dates) => setAvailableDates(dates || []))
        .catch(() => setAvailableDates([]))
        .finally(() => setLoadingDates(false));
    } else {
      setAvailableDates([]);
    }
  }, [form.doctorId, form.branchId]);

  useEffect(() => {
    const { doctorId, branchId, date } = form;
    if (doctorId && branchId && date) {
      setLoadingSlots(true);
      getAvailableSlots(doctorId, branchId, date)
        .then((slots) => setAvailableSlots(slots || []))
        .catch(() => setAvailableSlots([]))
        .finally(() => setLoadingSlots(false));
    } else {
      setAvailableSlots([]);
    }
  }, [form.doctorId, form.branchId, form.date]);

  const updateField = (field, value) => setForm((current) => ({ ...current, [field]: value }));

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSaving(true);
    try {
      const formattedTime = form.startTime.length === 5 ? `${form.startTime}:00` : form.startTime;
      await bookAppointment({
        ...form,
        patientId: Number(form.patientId),
        doctorId: Number(form.doctorId),
        serviceId: Number(form.serviceId),
        branchId: Number(form.branchId),
        startTime: formattedTime,
      });
      navigate("/moderator/appointments");
    } catch (requestError) {
      const errData = requestError.response?.data;
      const errorMsg = errData?.message || errData?.detail || requestError.message || "Unable to create appointment.";
      setError(errorMsg);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="appointment-form-page">
      <div className="form-header"><div><h1>Add Appointment</h1><p>Create a new clinic appointment.</p></div><button onClick={() => navigate("/moderator/appointments")}>Back</button></div>
      {error && <p className="form-error">{error}</p>}
      <form className="appointment-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-group" style={{ position: "relative" }}>
            <label>Patient</label>
            <input 
              value={patientSearch} 
              onChange={(event) => {
                setPatientSearch(event.target.value);
                updateField("patientId", "");
              }} 
              onKeyDown={(event) => {
                if (event.key === 'Enter') event.preventDefault();
              }}
              placeholder="Search and select patient..." 
              required={!form.patientId} 
            />
            {patientSearch.length >= 2 && !form.patientId && patients.length > 0 && (
              <ul style={{ position: "absolute", top: "100%", left: 0, right: 0, background: "rgba(15, 23, 42, 1)", border: "1px solid rgba(148, 163, 184, 0.18)", zIndex: 10, listStyle: "none", padding: 0, margin: 0, maxHeight: "150px", overflowY: "auto", borderRadius: "14px", boxShadow: "0 8px 16px rgba(0,0,0,0.4)", marginTop: "4px" }}>
                {patients.map((patient) => (
                  <li 
                    key={patient.id} 
                    style={{ padding: "12px 14px", cursor: "pointer", borderBottom: "1px solid rgba(148, 163, 184, 0.1)", color: "white", fontSize: "14px" }}
                    onMouseEnter={(e) => e.currentTarget.style.background = "rgba(148, 163, 184, 0.1)"}
                    onMouseLeave={(e) => e.currentTarget.style.background = "transparent"}
                    onClick={() => {
                      updateField("patientId", patient.id);
                      setPatientSearch(`${patient.fullName} ${patient.phone ? `(${patient.phone})` : ""}`);
                      setPatients([]);
                    }}
                  >
                    {patient.fullName} {patient.phone ? `(${patient.phone})` : ""}
                  </li>
                ))}
              </ul>
            )}
          </div>
          <div className="form-group"><label>Doctor</label><select value={form.doctorId} onChange={(event) => updateField("doctorId", event.target.value)} required><option value="">Select doctor</option>{doctors.map((doctor) => <option key={doctor.id} value={doctor.id}>{doctor.fullName}</option>)}</select></div>
          <div className="form-group"><label>Service</label><select value={form.serviceId} onChange={(event) => updateField("serviceId", event.target.value)} required><option value="">Select service</option>{services.map((service) => <option key={service.id} value={service.id}>{service.name}</option>)}</select></div>
          <div className="form-group">
            <label>Available Dates</label>
            <select value={form.date} onChange={(event) => { updateField("date", event.target.value); updateField("startTime", ""); }} disabled={!form.doctorId || !form.branchId || loadingDates} required>
              <option value="">{loadingDates ? "Loading..." : "Select Date"}</option>
              {availableDates.map(d => {
                const formatted = new Date(d).toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'short', year: 'numeric' });
                return <option key={d} value={d}>{formatted}</option>;
              })}
            </select>
          </div>
          <div className="form-group">
            <label>Time Slot</label>
            <select value={form.startTime} onChange={(event) => updateField("startTime", event.target.value)} disabled={!form.date || loadingSlots} required>
              <option value="">{loadingSlots ? "Loading..." : "Select Time"}</option>
              {availableSlots.map(slot => (
                <option key={slot.startTime} value={slot.startTime}>
                  {formatTo12Hour(slot.startTime)} - {formatTo12Hour(slot.endTime)}
                </option>
              ))}
            </select>
          </div>
          <div className="form-group"><label>Status</label><input value="Pending" readOnly /></div>
          <div className="form-group"><label>Branch</label><select value={form.branchId} onChange={(event) => updateField("branchId", event.target.value)} required><option value="">Select branch</option>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></div>
        </div>
        <div className="form-group full"><label>Notes</label><textarea rows="5" value={form.notes} onChange={(event) => updateField("notes", event.target.value)} placeholder="Write appointment notes..." /></div>
        <div className="form-actions"><button type="button" onClick={() => navigate("/moderator/appointments")}>Cancel</button><button type="submit" className="primary" disabled={saving}>{saving ? "Saving..." : "Save Appointment"}</button></div>
      </form>
    </div>
  );
}

export default AddAppointment;
