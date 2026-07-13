import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getAppointmentById, updateAppointment } from "../../../services/appointmentServices";
import { getBranches, getDoctors, getServices, getAvailableDates, getAvailableSlots } from "../../../services/publicServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import "./EditAppointment.css";

function EditAppointment() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [appointment, setAppointment] = useState(null);
  const [branches, setBranches] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [services, setServices] = useState([]);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(null);
  const [availableDates, setAvailableDates] = useState([]);
  const [loadingDates, setLoadingDates] = useState(false);
  const [availableSlots, setAvailableSlots] = useState([]);
  const [loadingSlots, setLoadingSlots] = useState(false);

  useEffect(() => {
    Promise.all([getAppointmentById(id), getBranches(), getDoctors(), getServices()])
      .then(([appointmentData, branchData, doctorData, serviceData]) => {
        setAppointment(appointmentData);
        setBranches(branchData || []);
        setDoctors(doctorData || []);
        setServices(serviceData || []);
        setForm({ patientId: appointmentData.patientId, doctorId: appointmentData.doctorId, serviceId: appointmentData.serviceId, branchId: appointmentData.branchId, date: appointmentData.date, startTime: appointmentData.startTime?.slice(0, 5) || "", notes: appointmentData.notes || "", previousAppointmentId: appointmentData.previousAppointmentId || null });
      })
      .catch((requestError) => setError(requestError.message || "Unable to load appointment details."));
  }, [id]);

  const updateField = (field, value) => setForm((current) => ({ ...current, [field]: value }));

  useEffect(() => {
    if (form) {
      const { doctorId, branchId } = form;
      if (doctorId && branchId) {
        setLoadingDates(true);
        getAvailableDates(doctorId, branchId)
          .then((dates) => {
            // Include existing date if it's not in the fetched dates
            const existingDate = appointment?.date;
            let loadedDates = dates || [];
            if (existingDate && !loadedDates.includes(existingDate)) {
              loadedDates = [existingDate, ...loadedDates];
            }
            setAvailableDates(loadedDates);
          })
          .catch(() => setAvailableDates([]))
          .finally(() => setLoadingDates(false));
      } else {
        setAvailableDates([]);
      }
    }
  }, [form?.doctorId, form?.branchId, appointment?.date]);

  useEffect(() => {
    if (form) {
      const { doctorId, branchId, date } = form;
      if (doctorId && branchId && date) {
        setLoadingSlots(true);
        getAvailableSlots(doctorId, branchId, date)
          .then((slots) => {
            // If the current appointment's date is selected, we need to ensure its existing time slot is an option
            let loadedSlots = slots || [];
            const existingDate = appointment?.date;
            const existingTime = appointment?.startTime?.slice(0, 5);
            if (date === existingDate && existingTime && !loadedSlots.some(s => s.startTime.startsWith(existingTime))) {
              loadedSlots = [{ startTime: appointment.startTime, endTime: appointment.endTime || appointment.startTime }, ...loadedSlots];
            }
            setAvailableSlots(loadedSlots);
          })
          .catch(() => setAvailableSlots([]))
          .finally(() => setLoadingSlots(false));
      } else {
        setAvailableSlots([]);
      }
    }
  }, [form?.doctorId, form?.branchId, form?.date, appointment]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      await updateAppointment(id, { ...form, patientId: Number(form.patientId), doctorId: Number(form.doctorId), serviceId: Number(form.serviceId), branchId: Number(form.branchId) });
      navigate(`/moderator/appointments/${id}`);
    } catch (requestError) {
      setError(requestError.message || "Unable to update appointment.");
    } finally {
      setSaving(false);
    }
  };

  if (error && !form) return <div className="appointment-form-page">{error}</div>;
  if (!form || !appointment) return <div className="appointment-form-page">Loading appointment...</div>;

  return (
    <div className="appointment-form-page">
      <div className="form-header"><div><h1>Edit Appointment</h1><p>Update appointment information.</p></div><button onClick={() => navigate("/moderator/appointments")}>Back</button></div>
      {error && <p className="form-error">{error}</p>}
      
      {["Completed", "Cancelled", "Rejected"].includes(appointment.status) && (
        <div className="form-warning" style={{ backgroundColor: "#fff3cd", color: "#856404", padding: "12px", borderRadius: "4px", marginBottom: "20px", border: "1px solid #ffeeba" }}>
          <strong>Warning:</strong> This appointment is {appointment.status} and can no longer be edited.
        </div>
      )}

      <form className="appointment-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-group"><label>Patient Name</label><input value={appointment.patientName} readOnly disabled={["Completed", "Cancelled", "Rejected"].includes(appointment.status)} /></div>
          <div className="form-group"><label>Doctor</label><select value={form.doctorId} onChange={(event) => updateField("doctorId", event.target.value)} required disabled={["Completed", "Cancelled", "Rejected"].includes(appointment.status)}>{doctors.map((doctor) => <option key={doctor.id} value={doctor.id}>{doctor.fullName}</option>)}</select></div>
          <div className="form-group"><label>Service</label><select value={form.serviceId} onChange={(event) => updateField("serviceId", event.target.value)} required disabled={["Completed", "Cancelled", "Rejected"].includes(appointment.status)}>{services.map((service) => <option key={service.id} value={service.id}>{service.name}</option>)}</select></div>
          
          <div className="form-group">
            <label>Available Dates</label>
            <select value={form.date} onChange={(event) => { updateField("date", event.target.value); updateField("startTime", ""); }} disabled={!form.doctorId || !form.branchId || loadingDates || ["Completed", "Cancelled", "Rejected"].includes(appointment.status)} required>
              <option value="">{loadingDates ? "Loading..." : "Select Date"}</option>
              {availableDates.map(d => {
                const formatted = new Date(d).toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'short', year: 'numeric' });
                return <option key={d} value={d}>{formatted}</option>;
              })}
            </select>
          </div>
          
          <div className="form-group">
            <label>Time Slot</label>
            <select value={form.startTime} onChange={(event) => updateField("startTime", event.target.value)} disabled={!form.date || loadingSlots || ["Completed", "Cancelled", "Rejected"].includes(appointment.status)} required>
              <option value="">{loadingSlots ? "Loading..." : "Select Time"}</option>
              {availableSlots.map(slot => {
                const sTime = slot.startTime.slice(0, 5);
                return (
                  <option key={sTime} value={sTime}>
                    {formatTo12Hour(slot.startTime)} - {formatTo12Hour(slot.endTime)}
                  </option>
                );
              })}
            </select>
          </div>

          <div className="form-group"><label>Status</label><input value={appointment.status} readOnly disabled={["Completed", "Cancelled", "Rejected"].includes(appointment.status)} /></div>
          <div className="form-group"><label>Branch</label><select value={form.branchId} onChange={(event) => updateField("branchId", event.target.value)} required disabled={["Completed", "Cancelled", "Rejected"].includes(appointment.status)}>{branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></div>
        </div>
        <div className="form-group full"><label>Notes</label><textarea rows="5" value={form.notes} onChange={(event) => updateField("notes", event.target.value)} disabled={["Completed", "Cancelled", "Rejected"].includes(appointment.status)} /></div>
        <div className="form-actions"><button type="button" onClick={() => navigate("/moderator/appointments")}>Cancel</button><button type="submit" className="primary" disabled={saving || ["Completed", "Cancelled", "Rejected"].includes(appointment.status)}>{saving ? "Saving..." : "Update Appointment"}</button></div>
      </form>
    </div>
  );
}

export default EditAppointment;
