import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import "./BookAppointment.css";

import { 
  getBranches, 
  getServices, 
  getDoctors, 
  getAvailableSlots 
} from "../../../services/publicServices";
import { bookAppointment } from "../../../services/appointmentServices";

import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import Input from "../../../components/common/Input";
import Loading from "../../../components/common/Loading/Loading";

function BookAppointment() {
  const navigate = useNavigate();

  // Reference Data State
  const [branches, setBranches] = useState([]);
  const [services, setServices] = useState([]);
  const [doctors, setDoctors] = useState([]);
  
  // UI State
  const [loadingData, setLoadingData] = useState(true);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  // Form State
  const [branchId, setBranchId] = useState("");
  const [serviceId, setServiceId] = useState("");
  const [doctorId, setDoctorId] = useState("");
  const [date, setDate] = useState("");
  const [notes, setNotes] = useState("");
  
  // Time Slots State
  const [availableSlots, setAvailableSlots] = useState([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState(""); // Stores startTime

  const loadInitialData = useCallback(async () => {
    try {
      setLoadingData(true);
      setError(null);
      const [branchesData, servicesData, doctorsData] = await Promise.all([
        getBranches(),
        getServices(),
        getDoctors()
      ]);

      setBranches(branchesData || []);
      setServices(servicesData || []);
      setDoctors(doctorsData || []);
    } catch (err) {
      setError("Failed to load clinic information. Please try again later.");
      console.error(err);
    } finally {
      setLoadingData(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadInitialData();
  }, [loadInitialData]);

  const fetchSlots = useCallback(async () => {
    if (!branchId || !doctorId || !date) {
      setAvailableSlots([]);
      setSelectedSlot("");
      return;
    }

    try {
      setLoadingSlots(true);
      setSelectedSlot(""); // reset selected slot when changing date/doc
      const slots = await getAvailableSlots(doctorId, branchId, date);
      setAvailableSlots(slots || []);
    } catch (err) {
      console.error("Failed to load time slots", err);
      setAvailableSlots([]);
    } finally {
      setLoadingSlots(false);
    }
  }, [branchId, doctorId, date]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchSlots();
  }, [fetchSlots]);

  const handleSubmit = async () => {
    setError(null);

    // Frontend validation
    if (!branchId || !serviceId || !doctorId || !date || !selectedSlot) {
      setError("Please fill out all required fields and select a time slot.");
      return;
    }

    try {
      setSubmitting(true);
      const payload = {
        doctorId: parseInt(doctorId, 10),
        branchId: parseInt(branchId, 10),
        serviceId: parseInt(serviceId, 10),
        date: date,
        startTime: selectedSlot,
        notes: notes.trim() ? notes : null
      };

      await bookAppointment(payload);
      
      // On success, navigate back
      navigate("/patient/appointments");
    } catch (err) {
      setError(err.message || "Failed to book appointment.");
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingData) return <div className="book-appointment-page"><Loading /></div>;

  // Options mapping
  const branchOptions = [
    { value: "", label: "Select Branch" },
    ...branches.map(b => ({ value: b.id.toString(), label: b.name }))
  ];
  
  const serviceOptions = [
    { value: "", label: "Select Service" },
    ...services.map(s => ({ value: s.id.toString(), label: s.name }))
  ];

  const doctorOptions = [
    { value: "", label: "Select Doctor" },
    ...doctors.map(d => ({ value: d.id.toString(), label: d.fullName }))
  ];

  // Get today's date formatted as YYYY-MM-DD for the min attribute
  const today = new Date().toISOString().split("T")[0];

  return (
    <div className="book-appointment-page">
      <button className="back-btn" onClick={() => navigate("/patient/appointments")}>
        &larr; Back to Appointments
      </button>

      <div className="book-appointment-header">
        <h1>Book Appointment</h1>
        <p>Schedule a new visit by selecting the required details below.</p>
      </div>

      <div className="book-appointment-form">
        {error && <div className="form-error">{error}</div>}

        <div className="form-row">
          <div className="form-group">
            <FilterDropdown
              label="Branch *"
              options={branchOptions}
              value={branchId}
              onChange={setBranchId}
              disabled={submitting}
            />
          </div>
          <div className="form-group">
            <FilterDropdown
              label="Service *"
              options={serviceOptions}
              value={serviceId}
              onChange={setServiceId}
              disabled={submitting}
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <FilterDropdown
              label="Doctor *"
              options={doctorOptions}
              value={doctorId}
              onChange={setDoctorId}
              disabled={submitting}
            />
          </div>
          <div className="form-group">
            <Input
              label="Date *"
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              min={today}
              disabled={submitting}
            />
          </div>
        </div>

        {date && doctorId && branchId && (
          <div className="time-slots-container">
            <h3>Available Time Slots *</h3>
            
            {loadingSlots ? (
              <p style={{ color: "#94a3b8", fontSize: "14px" }}>Loading slots...</p>
            ) : availableSlots.length === 0 ? (
              <p style={{ color: "#ef4444", fontSize: "14px" }}>
                No available slots for the selected doctor on this date. Please try another date.
              </p>
            ) : (
              <div className="time-slots-grid">
                {availableSlots.map((slot, index) => (
                  <button
                    key={index}
                    className={`time-slot-btn ${selectedSlot === slot.startTime ? "selected" : ""}`}
                    onClick={() => setSelectedSlot(slot.startTime)}
                    disabled={submitting}
                    type="button"
                  >
                    {/* Assuming startTime is HH:MM:SS or HH:MM string */}
                    {slot.startTime.substring(0, 5)}
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        <div className="form-row">
          <div className="form-group" style={{ flex: "100%" }}>
            <label>Notes (Optional)</label>
            <textarea
              placeholder="Any specific symptoms or questions for the doctor?"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              disabled={submitting}
            />
          </div>
        </div>

        <div className="form-actions">
          <button 
            className="submit-booking-btn" 
            onClick={handleSubmit}
            disabled={submitting || !branchId || !serviceId || !doctorId || !date || !selectedSlot}
          >
            {submitting ? "Booking..." : "Confirm Booking"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default BookAppointment;
