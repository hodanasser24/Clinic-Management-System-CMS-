import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Appointments.css";


import StatusTabs from "../../../components/common/StatusTabs/StatusTabs";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import CancelModal from "../../../components/ui/CancelModal/CancelModal";
import { getAllAppointments, cancelAppointment } from "../../../services/appointmentServices";
import { getDoctors } from "../../../services/publicServices";

function Appointments() {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState("All");

  const tabs = [
    "All",
    "Pending",
    "Confirmed",
    "Completed",
    "Rejected",
    "Cancelled",
  ];

  const columns = [
    { key: "id", label: "ID" },
    { key: "patientName", label: "Patient" },
    { key: "doctorName", label: "Doctor" },
    { key: "date", label: "Date" },
    { key: "status", label: "Status" },
  ];

  const [appointments, setAppointments] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(true);

  // Modal State
  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [appointmentToCancel, setAppointmentToCancel] = useState(null);

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [doctorFilter, setDoctorFilter] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const fetchAppointments = async () => {
    try {
      setLoading(true);
      const params = {
        page: currentPage,
        pageSize: 10,
        status: activeTab !== "All" ? activeTab : undefined,
        doctorId: doctorFilter || undefined,
        sortBy: sortBy === "oldest" ? "CreatedAt" : sortBy === "newest" ? "CreatedAt" : undefined,
        sortDescending: sortBy !== "oldest",
      };

      if (searchQuery) {
        params.fromDate = searchQuery;
        params.toDate = searchQuery;
      }

      const data = await getAllAppointments(params);
      setAppointments(data?.items || []);
      setTotalCount(data?.totalCount || 0);
    } catch (err) {
      console.error("Failed to fetch appointments", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAppointments();
  }, [currentPage, activeTab, doctorFilter, searchQuery, sortBy]);

  useEffect(() => {
    getDoctors().then((data) => setDoctors(data || [])).catch((error) => console.error("Failed to fetch doctors", error));
  }, []);

  const initiateCancel = (id) => {
    setAppointmentToCancel(id);
    setCancelModalOpen(true);
  };

  const handleCancel = async (reason) => {
    try {
      await cancelAppointment(appointmentToCancel, reason);
      setCancelModalOpen(false);
      setAppointmentToCancel(null);
      fetchAppointments();
    } catch (err) {
      const errData = err.response?.data;
      const errorMsg = errData?.message || errData?.detail || err.message || "Unknown error";
      alert("Cancellation failed: " + errorMsg);
    }
  };

  const filtered = appointments;

  // 5. Pagination calculation for UI
  const itemsPerPage = 10;
  const totalPages = Math.max(Math.ceil(totalCount / itemsPerPage), 1);

  return (
    <div className="appointments-page">
      <div className="appointments-header">
        <div>
          <h1>Appointments</h1>
          <p>Manage clinic appointments, status, and patient bookings.</p>
        </div>

        <button
          className="add-appointment-btn"
          onClick={() => navigate("/moderator/appointments/add")}
        >
          + Add Appointment
        </button>
      </div>

      <div className="appointments-toolbar">
        <div className="search-input">
          <span>📅</span>
          <input
            type="date"
            value={searchQuery}
            onChange={(e) => {
              setSearchQuery(e.target.value);
              setCurrentPage(1);
            }}
          />
          {searchQuery && (
            <button onClick={() => { setSearchQuery(""); setCurrentPage(1); }}>Clear</button>
          )}
        </div>

        <FilterDropdown
          label="Doctor"
          value={doctorFilter}
          onChange={(val) => {
            setDoctorFilter(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "All Doctors" },
            ...doctors.map((doctor) => ({ value: String(doctor.id), label: doctor.fullName })),
          ]}
        />

        <SortDropdown
          value={sortBy}
          onChange={(val) => {
            setSortBy(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "Sort By" },
            { value: "newest", label: "Newest Booking" },
            { value: "oldest", label: "Oldest Booking" },
          ]}
        />
      </div>

      <StatusTabs
        tabs={tabs}
        activeTab={activeTab}
        onChange={(tab) => {
          setActiveTab(tab);
          setCurrentPage(1);
        }}
      />

      {loading ? (
        <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>
          Loading appointments...
        </div>
      ) : (
        <DataTable
          columns={columns}
          data={filtered}
          actions={(row) => (
            <div className="table-actions">
              <button
                onClick={() => navigate(`/moderator/appointments/${row.id}`)}
              >
                View
              </button>

              <button
                onClick={() => navigate(`/moderator/appointments/edit/${row.id}`)}
              >
                Edit
              </button>

            </div>
          )}
        />
      )}

      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={(page) => setCurrentPage(page)}
      />

      <CancelModal
        isOpen={cancelModalOpen}
        onClose={() => {
          setCancelModalOpen(false);
          setAppointmentToCancel(null);
        }}
        onConfirm={handleCancel}
      />
    </div>
  );
}

export default Appointments;
