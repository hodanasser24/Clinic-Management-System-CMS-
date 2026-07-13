import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getUserId } from "../../../services/authServices";
import { getDoctorAppointments } from "../../../services/appointmentServices";
import SearchInput from "../../../components/common/SearchInput/SearchInput";
import StatusTabs from "../../../components/common/StatusTabs/StatusTabs";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import "./Appointments.css";

function Appointments() {
  const navigate = useNavigate();
  const userId = getUserId();

  const tabs = ["All", "Today", "Upcoming", "Completed", "Cancelled"];

  const columns = [
    { key: "patientName", label: "Patient" },
    { key: "date", label: "Date" },
    { key: "time", label: "Time" },
    { key: "serviceName", label: "Service" },
    { key: "status", label: "Status" },
  ];

  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);

  // Filtering and Sorting States
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [dateFilter, setDateFilter] = useState("");
  const [sortBy, setSortBy] = useState("Date");
  const [sortDescending, setSortDescending] = useState(true);
  const [activeTab, setActiveTab] = useState("All");
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  const loadAppointments = async () => {
    setLoading(true);
    try {
      const params = {
        page: currentPage,
        pageSize: itemsPerPage,
        sortBy: sortBy,
        sortDescending: sortDescending,
      };

      // Map Active Tab
      const todayStr = new Date().toISOString().split("T")[0];
      if (activeTab === "Today") {
        params.fromDate = todayStr;
        params.toDate = todayStr;
      } else if (activeTab === "Upcoming") {
        params.fromDate = todayStr;
        // In a perfect world we'd pass multiple statuses (Pending/Confirmed), but DTO only takes one.
        // We'll rely on fromDate for upcoming.
      } else if (activeTab === "Completed") {
        params.status = 2; // Assuming 2 is Completed
      } else if (activeTab === "Cancelled") {
        params.status = 3; // Assuming 3 is Cancelled
      }

      // Dropdown Overrides (takes precedence over tabs if set)
      if (statusFilter !== "") {
        params.status = statusFilter;
      }
      
      if (dateFilter === "today") {
        params.fromDate = todayStr;
        params.toDate = todayStr;
      } else if (dateFilter === "upcoming") {
        params.fromDate = todayStr;
      }

      // Add search
      if (searchQuery) {
        params.patientName = searchQuery; // DTO uses PatientName
      }

      const res = await getDoctorAppointments(userId, params);
      setAppointments(res?.items || res || []);
      setTotalCount(res?.totalCount || 0);
    } catch (error) {
      console.error("Failed to load doctor appointments", error);
      setAppointments([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (userId) {
      loadAppointments();
    }
  }, [userId, currentPage, activeTab, statusFilter, dateFilter, sortBy, sortDescending, searchQuery]);

  const totalPages = Math.max(Math.ceil(totalCount / itemsPerPage), 1);

  // Map API data to table format
  const tableData = appointments.map((app) => ({
    id: app.id,
    patientId: app.patientId,
    patientName: app.patientName,
    date: app.date,
    time: formatTo12Hour(app.startTime),
    serviceName: app.serviceName,
    status: (
      <span className={`status-badge ${getEnumStatusString(app.status)?.toLowerCase()}`}>
        {getEnumStatusString(app.status)}
      </span>
    ),
  }));

  function getEnumStatusString(statusValue) {
    switch (statusValue) {
      case 0: return "Pending";
      case 1: return "Confirmed";
      case 2: return "Completed";
      case 3: return "Cancelled";
      case "Pending": return "Pending";
      case "Confirmed": return "Confirmed";
      case "Completed": return "Completed";
      case "Cancelled": return "Cancelled";
      default: return "Unknown";
    }
  }

  return (
    <div className="doctor-appointments-page">
      <div className="doctor-appointments-header">
        <div>
          <h1>Doctor Appointments</h1>
          <p>Review today’s appointments and patient visits.</p>
        </div>
      </div>

      <div className="doctor-appointments-toolbar">
        <SearchInput
          placeholder="Search patient..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onSearch={() => setCurrentPage(1)}
        />

        <FilterDropdown
          label="Status"
          value={statusFilter}
          onChange={(val) => {
            setStatusFilter(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "All Status" },
            { value: "0", label: "Pending" },
            { value: "1", label: "Confirmed" },
            { value: "2", label: "Completed" },
            { value: "3", label: "Cancelled" },
          ]}
        />

        <FilterDropdown
          label="Date"
          value={dateFilter}
          onChange={(val) => {
            setDateFilter(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "All Dates" },
            { value: "today", label: "Today" },
            { value: "upcoming", label: "Upcoming" },
          ]}
        />

        <SortDropdown
          value={sortBy === "Date" ? (sortDescending ? "dateDesc" : "dateAsc") : ""}
          onChange={(val) => {
            if (val === "dateAsc") {
              setSortBy("Date");
              setSortDescending(false);
            } else if (val === "dateDesc") {
              setSortBy("Date");
              setSortDescending(true);
            } else {
              setSortBy("");
            }
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "Sort By" },
            { value: "dateAsc", label: "Date Ascending" },
            { value: "dateDesc", label: "Date Descending" },
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
        <p>Loading appointments...</p>
      ) : (
        <DataTable
          columns={columns}
          data={tableData}
          actions={(row) => (
            <div className="doctor-table-actions">
              <button onClick={() => navigate(`/doctor/appointments/${row.id}`)}>
                View
              </button>

              <button onClick={() => navigate(`/doctor/patients/${row.patientId}`)}>
                Record
              </button>

              <button onClick={() => navigate(`/doctor/prescriptions?patientId=${row.patientId}`)}>
                Prescription
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
    </div>
  );
}

export default Appointments;
