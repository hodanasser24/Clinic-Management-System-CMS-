import { useState, useEffect, useMemo, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import "./Appointments.css"; // Uses local which imports shared moderator styles

import SearchInput from "../../../components/common/SearchInput/SearchInput";
import StatusTabs from "../../../components/common/StatusTabs/StatusTabs";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import EmptyState from "../../../components/common/EmptyState/EmptyState";
import Loading from "../../../components/common/Loading/Loading";

import { 
  getUpcomingPatientAppointments, 
  getHistoryPatientAppointments 
} from "../../../services/appointmentServices";
import { getCurrentPatientId } from "../../../services/authServices";

function Appointments() {
  const navigate = useNavigate();
  const [viewType, setViewType] = useState("Upcoming");
  const [statusFilter, setStatusFilter] = useState("");
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDoctor, setSelectedDoctor] = useState("");
  const [sortBy, setSortBy] = useState("newest");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const columns = [
    { key: "id", label: "ID" },
    { key: "doctorName", label: "Doctor" },
    { key: "serviceName", label: "Service" },
    { key: "date", label: "Date" },
    { key: "status", label: "Status" },
  ];

  const fetchAppointments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const patientId = getCurrentPatientId();
      // Fetching a large page size to handle frontend filtering/searching
      let response;
      if (viewType === "Upcoming") {
        response = await getUpcomingPatientAppointments(patientId, { page: currentPage, pageSize: 100 });
      } else {
        response = await getHistoryPatientAppointments(patientId, { page: currentPage, pageSize: 100 });
      }
      
      setAppointments(response?.items || []);
      setTotalPages(response?.totalPages || 1);
    } catch (err) {
      setError("Failed to load appointments. Please try again later.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [currentPage, viewType]);

  useEffect(() => {
    // Reset page and status filter when switching views
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setCurrentPage(1);
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setStatusFilter("");
  }, [viewType]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchAppointments();
  }, [fetchAppointments]);

  // Frontend Search, Filter, and Sort logic
  const filteredAndSortedData = useMemo(() => {
    let result = [...appointments];

    // Status filter
    if (statusFilter) {
      result = result.filter(
        (app) => app.status === statusFilter
      );
    }

    // Search query filter (by doctor name or service)
    if (searchQuery) {
      const lowerQuery = searchQuery.toLowerCase();
      result = result.filter(
        (app) =>
          app.doctorName?.toLowerCase().includes(lowerQuery) ||
          app.serviceName?.toLowerCase().includes(lowerQuery)
      );
    }

    // Doctor filter dropdown
    if (selectedDoctor) {
      result = result.filter((app) => app.doctorName === selectedDoctor);
    }

    // Sort logic
    result.sort((a, b) => {
      const dateA = new Date(`${a.date}T${a.startTime}`);
      const dateB = new Date(`${b.date}T${b.startTime}`);
      if (sortBy === "newest") {
        return dateB - dateA; // Newest first
      } else {
        return dateA - dateB; // Oldest first
      }
    });

    return result;
  }, [appointments, statusFilter, searchQuery, selectedDoctor, sortBy]);

  // Extract unique doctors for the filter dropdown
  const doctorOptions = useMemo(() => {
    const doctors = [...new Set(appointments.map((app) => app.doctorName))].filter(Boolean);
    return [
      { value: "", label: "All Doctors" },
      ...doctors.map((doc) => ({ value: doc, label: doc })),
    ];
  }, [appointments]);

  const statusOptions = [
    { value: "", label: "All Statuses" },
    ...(viewType === "Upcoming"
      ? [
          { value: "Pending", label: "Pending" },
          { value: "Confirmed", label: "Confirmed" },
          { value: "Rescheduled", label: "Rescheduled" },
        ]
      : [
          { value: "Completed", label: "Completed" },
          { value: "Rejected", label: "Rejected" },
          { value: "Cancelled", label: "Cancelled" },
        ]),
  ];

  const handleSearch = () => {
    // Applied via useMemo automatically
  };

  return (
    <div className="appointments-page">
      <div className="appointments-header">
        <div>
          <h1>My Appointments</h1>
          <p>Manage your clinic appointments, status, and bookings.</p>
        </div>

        <button 
          className="add-appointment-btn" 
          onClick={() => navigate("/patient/appointments/book")}
        >
          + Book Appointment
        </button>
      </div>

      <div className="appointments-toolbar">
        <SearchInput
          placeholder="Search by doctor or service..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onSearch={handleSearch}
        />

        <FilterDropdown
          label="Doctor"
          value={selectedDoctor}
          onChange={setSelectedDoctor}
          options={doctorOptions}
        />

        <FilterDropdown
          label="Status"
          value={statusFilter}
          onChange={setStatusFilter}
          options={statusOptions}
        />

        <SortDropdown
          value={sortBy}
          onChange={setSortBy}
          options={[
            { value: "newest", label: "Newest Booking" },
            { value: "oldest", label: "Oldest Booking" },
          ]}
        />
      </div>

      <StatusTabs tabs={["Upcoming", "History"]} activeTab={viewType} onChange={setViewType} />

      {loading ? (
        <Loading />
      ) : error ? (
        <EmptyState title="Error" message={error} />
      ) : filteredAndSortedData.length === 0 ? (
        <EmptyState title="No Appointments Found" message="Try changing your search or filters." />
      ) : (
        <DataTable
          columns={columns}
          data={filteredAndSortedData.map((app) => ({
            ...app,
            // Format status for rendering if needed, though raw string is fine
            status: <span className={`status-badge ${app.status?.toLowerCase()}`}>{app.status}</span>
          }))}
          actions={(row) => (
            <div className="table-actions">
              <button onClick={() => navigate(`/patient/appointments/${row.id}`)}>
                View Details
              </button>
            </div>
          )}
        />
      )}

      {!loading && !error && filteredAndSortedData.length > 0 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      )}
    </div>
  );
}

export default Appointments;
