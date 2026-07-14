import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getUserId } from "../../../services/authServices";
import {
  getPatientAppointments,
  cancelAppointment,
  bookAppointment
} from "../../../services/appointmentServices";
import {
  getBranches,
  getServices,
  getDoctors,
  getAvailableSlots,
  getAvailableDates
} from "../../../services/publicServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import { getFriendlyErrorMessage } from "../../../utils/errorMapper";
import CancelModal from "../../../components/ui/CancelModal/CancelModal";
import "./PatientAppointments.css";

function PatientAppointments() {
  const navigate = useNavigate();
  const userId = getUserId();

  // Lists
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [branches, setBranches] = useState([]);
  const [services, setServices] = useState([]);
  const [doctors, setDoctors] = useState([]);

  // Filters & State
  const [doctorQuery, setDoctorQuery] = useState("");
  const [dateQuery, setDateQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Booking Wizard State
  const [isBookingOpen, setIsBookingOpen] = useState(false);
  const [bookingForm, setBookingForm] = useState({
    branchId: "",
    serviceId: "",
    doctorId: "",
    date: "",
    startTime: "",
    notes: ""
  });
  const [availableDates, setAvailableDates] = useState([]);
  const [loadingDates, setLoadingDates] = useState(false);
  const [availableSlots, setAvailableSlots] = useState([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [bookingError, setBookingError] = useState("");
  const [bookingSuccess, setBookingSuccess] = useState(false);

  // Cancellation State
  const [isCancelOpen, setIsCancelOpen] = useState(false);
  const [cancelId, setCancelId] = useState(null);
  const [cancelError, setCancelError] = useState("");

  // Details Modal State
  const [selectedAppt, setSelectedAppt] = useState(null);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);

  const loadAppointments = async () => {
    setLoading(true);
    try {
      const params = {
        page: page,
        pageSize: 10,
        sortDescending: true
      };
      
      if (doctorQuery) params.doctorName = doctorQuery;
      if (dateQuery) {
        params.fromDate = dateQuery;
        params.toDate = dateQuery;
      }
      if (statusFilter && statusFilter !== "All") {
        const statusMap = {
          "Pending": 0,
          "Confirmed": 1,
          "Cancelled": 3,
          "Completed": 4
        };
        params.status = statusMap[statusFilter];
      }

      const res = await getPatientAppointments(userId, params);
      setAppointments(res?.items || []);
      setTotalPages(Math.ceil((res?.totalCount || 0) / 10));
    } catch (err) {
      console.error("Failed to load appointments:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAppointments();
  }, [userId, page, doctorQuery, dateQuery, statusFilter]);

  useEffect(() => {
    // Load lists for wizard
    getBranches().then(setBranches).catch(console.error);
    getServices().then(setServices).catch(console.error);
    getDoctors().then(setDoctors).catch(console.error);
  }, [userId]);

  // Load available dates when doctor and branch change
  useEffect(() => {
    const { doctorId, branchId } = bookingForm;
    if (doctorId && branchId) {
      setLoadingDates(true);
      getAvailableDates(doctorId, branchId)
        .then((dates) => {
          setAvailableDates(dates || []);
        })
        .catch((err) => {
          console.error("Error loading dates:", err);
          setAvailableDates([]);
        })
        .finally(() => setLoadingDates(false));
    } else {
      setAvailableDates([]);
    }
  }, [bookingForm.doctorId, bookingForm.branchId]);

  // Load slots when date selection changes
  useEffect(() => {
    const { doctorId, branchId, date } = bookingForm;
    if (doctorId && branchId && date) {
      setLoadingSlots(true);
      getAvailableSlots(doctorId, branchId, date)
        .then((slots) => {
          setAvailableSlots(slots || []);
        })
        .catch((err) => {
          console.error("Error loading slots:", err);
          setAvailableSlots([]);
        })
        .finally(() => setLoadingSlots(false));
    } else {
      setAvailableSlots([]);
    }
  }, [bookingForm.doctorId, bookingForm.branchId, bookingForm.date]);

  const handleBookingSubmit = async (e) => {
    e.preventDefault();
    setBookingError("");
    setBookingSuccess(false);

    const { branchId, serviceId, doctorId, date, startTime, notes } = bookingForm;
    if (!branchId || !serviceId || !doctorId || !date || !startTime) {
      setBookingError("Please fill out all required fields.");
      return;
    }

    try {
      await bookAppointment({
        patientId: userId,
        doctorId: parseInt(doctorId, 10),
        branchId: parseInt(branchId, 10),
        serviceId: parseInt(serviceId, 10),
        date,
        startTime,
        notes
      });
      setBookingSuccess(true);
      setTimeout(() => {
        setIsBookingOpen(false);
        setBookingForm({
          branchId: "",
          serviceId: "",
          doctorId: "",
          date: "",
          startTime: "",
          notes: ""
        });
        setBookingSuccess(false);
        loadAppointments();
      }, 1500);
    } catch (err) {
      setBookingError(err.message || "Booking failed.");
    }
  };

  const handleCancelConfirm = async (reason) => {
    setCancelError("");
    try {
      await cancelAppointment(cancelId, reason);
      setIsCancelOpen(false);
      loadAppointments();
    } catch (err) {
      setCancelError(getFriendlyErrorMessage(err));
    }
  };

  // Filtering is now handled completely by the server
  const filtered = appointments;

  return (
    <div className="patient-appointments-page">
      <div className="page-header">
        <h1>My Appointments</h1>
        <button onClick={() => setIsBookingOpen(true)}>
          + Book Appointment
        </button>
      </div>

      <div className="toolbar">
        <input
          type="text"
          placeholder="Search by Doctor Name..."
          value={doctorQuery}
          onChange={(e) => { setDoctorQuery(e.target.value); setPage(1); }}
        />

        <input
          type="date"
          value={dateQuery}
          onChange={(e) => { setDateQuery(e.target.value); setPage(1); }}
        />

        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
        >
          <option value="">All Statuses</option>
          <option value="Pending">Pending</option>
          <option value="Confirmed">Confirmed</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      {loading ? (
        <div style={{ textAlign: "center", padding: "3rem", color: "var(--text-secondary)" }}>
          Loading appointments...
        </div>
      ) : filtered.length === 0 ? (
        <div style={{ textAlign: "center", padding: "3rem", color: "var(--text-secondary)", backgroundColor: "rgba(255,255,255,0.02)", borderRadius: "8px" }}>
          No appointments found matching the filters.
        </div>
      ) : (
        <>
          <table className="appointments-table">
            <thead>
              <tr>
                <th>Doctor</th>
                <th>Service</th>
                <th>Branch</th>
                <th>Date</th>
                <th>Time</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>

            <tbody>
              {filtered.map((item) => (
                <tr key={item.id}>
                  <td>Dr. {item.doctorName}</td>
                  <td>{item.serviceName}</td>
                  <td>{item.branchName}</td>
                  <td>{item.date}</td>
                  <td>{formatTo12Hour(item.startTime)}</td>
                  <td>
                    <span className={`status ${item.status.toLowerCase()}`}>{item.status}</span>
                  </td>
                  <td>
                    <button
                      onClick={() => {
                        setSelectedAppt(item);
                        setIsDetailsOpen(true);
                      }}
                    >
                      View
                    </button>

                    {["Pending", "Confirmed"].includes(item.status) && (
                      <button
                        className="cancel"
                        onClick={() => {
                          setCancelId(item.id);
                          setCancelError("");
                          setIsCancelOpen(true);
                        }}
                      >
                        Cancel
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          
          {totalPages > 1 && (
            <div className="pagination" style={{ display: "flex", justifyContent: "center", gap: "1rem", marginTop: "1rem" }}>
              <button disabled={page === 1} onClick={() => setPage((p) => p - 1)}>
                Previous
              </button>
              <span style={{ padding: "0.5rem" }}>
                Page {page} of {totalPages}
              </span>
              <button disabled={page === totalPages} onClick={() => setPage((p) => p + 1)}>
                Next
              </button>
            </div>
          )}
        </>
      )}

      {/* Booking Wizard Modal */}
      {isBookingOpen && (
        <div className="modal-overlay">
          <div className="modal-content glass-card">
            <h2>Book New Appointment</h2>
            <form onSubmit={handleBookingSubmit}>
              {bookingError && <div className="modal-error">{bookingError}</div>}
              {bookingSuccess && (
                <div className="modal-success">Booking Confirmed successfully!</div>
              )}

              <div className="form-group">
                <label>Branch *</label>
                <select
                  value={bookingForm.branchId}
                  onChange={(e) =>
                    setBookingForm({ ...bookingForm, branchId: e.target.value })
                  }
                  required
                >
                  <option value="">Select Branch</option>
                  {branches.map((b) => (
                    <option key={b.id} value={b.id}>
                      {b.name} - {b.location}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Service *</label>
                <select
                  value={bookingForm.serviceId}
                  onChange={(e) =>
                    setBookingForm({ ...bookingForm, serviceId: e.target.value })
                  }
                  required
                >
                  <option value="">Select Service</option>
                  {services.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name} - ${s.price} ({s.estimatedDurationMinutes}m)
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Doctor *</label>
                <select
                  value={bookingForm.doctorId}
                  onChange={(e) =>
                    setBookingForm({ ...bookingForm, doctorId: e.target.value })
                  }
                  required
                >
                  <option value="">Select Doctor</option>
                  {doctors.map((d) => (
                    <option key={d.id} value={d.id}>
                      Dr. {d.fullName} - {d.specialization}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Available Dates *</label>
                <select
                  value={bookingForm.date}
                  onChange={(e) =>
                    setBookingForm({ ...bookingForm, date: e.target.value, startTime: "" })
                  }
                  disabled={!bookingForm.doctorId || !bookingForm.branchId || loadingDates}
                  required
                >
                  <option value="">
                    {loadingDates ? "Loading dates..." : "Select Date"}
                  </option>
                  {availableDates.map((d, index) => {
                    const dateObj = new Date(d);
                    const formatted = dateObj.toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'short', year: 'numeric' });
                    return (
                      <option key={index} value={d}>
                        {formatted}
                      </option>
                    );
                  })}
                </select>
              </div>

              <div className="form-group">
                <label>Time Slot *</label>
                <select
                  value={bookingForm.startTime}
                  onChange={(e) =>
                    setBookingForm({ ...bookingForm, startTime: e.target.value })
                  }
                  disabled={!bookingForm.doctorId || !bookingForm.branchId || !bookingForm.date || loadingSlots}
                  required
                >
                  <option value="">
                    {loadingSlots ? "Loading slots..." : "Select Time"}
                  </option>
                  {availableSlots.map((slot, index) => (
                    <option key={index} value={slot.startTime}>
                      {formatTo12Hour(slot.startTime)} - {formatTo12Hour(slot.endTime)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Notes</label>
                <textarea
                  placeholder="Any symptoms or notes..."
                  value={bookingForm.notes}
                  onChange={(e) =>
                    setBookingForm({ ...bookingForm, notes: e.target.value })
                  }
                />
              </div>

              <div className="modal-actions">
                <button type="button" className="secondary-btn" onClick={() => setIsBookingOpen(false)}>
                  Cancel
                </button>
                <button type="submit" disabled={bookingSuccess}>
                  Confirm Booking
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Cancellation Modal */}
      <CancelModal
        isOpen={isCancelOpen}
        onClose={() => { setIsCancelOpen(false); setCancelError(""); }}
        onConfirm={handleCancelConfirm}
        errorMessage={cancelError}
      />

      {/* Details View Modal */}
      {isDetailsOpen && selectedAppt && (
        <div className="modal-overlay">
          <div className="modal-content glass-card">
            <h2>Appointment Details</h2>
            <div className="details-grid" style={{ margin: "1.5rem 0", display: "grid", gap: "1rem" }}>
              <div><strong>Doctor:</strong> Dr. {selectedAppt.doctorName}</div>
              <div><strong>Service:</strong> {selectedAppt.serviceName}</div>
              <div><strong>Branch:</strong> {selectedAppt.branchName}</div>
              <div><strong>Date:</strong> {selectedAppt.date}</div>
              <div><strong>Time:</strong> {formatTo12Hour(selectedAppt.startTime)}</div>
              <div><strong>Status:</strong> <span className={`status ${selectedAppt.status.toLowerCase()}`}>{selectedAppt.status}</span></div>
              {selectedAppt.notes && <div><strong>Notes:</strong> {selectedAppt.notes}</div>}
            </div>
            <div className="modal-actions">
              <button className="secondary-btn" onClick={() => setIsDetailsOpen(false)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default PatientAppointments;
