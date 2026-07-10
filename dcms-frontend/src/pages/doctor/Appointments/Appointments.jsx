import { useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchInput from "../../../components/common/SearchInput/SearchInput";
import StatusTabs from "../../../components/common/StatusTabs/StatusTabs";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import "./Appointments.css";

function Appointments() {
  const navigate = useNavigate();

  const tabs = ["All", "Today", "Upcoming", "Completed", "Cancelled"];

  const columns = [
    { key: "patient", label: "Patient" },
    { key: "time", label: "Time" },
    { key: "service", label: "Service" },
    { key: "status", label: "Status" },
  ];

  const [appointments] = useState([
    {
      id: 1,
      patient: "Ahmed Ali",
      time: "10:00 AM",
      service: "Teeth Cleaning",
      status: "Pending",
      timestamp: 1000,
    },
    {
      id: 2,
      patient: "Mona Hassan",
      time: "11:30 AM",
      service: "Root Canal",
      status: "Confirmed",
      timestamp: 1130,
    },
    {
      id: 3,
      patient: "Omar Mohamed",
      time: "01:00 PM",
      service: "Consultation",
      status: "Completed",
      timestamp: 1300,
    },
    {
      id: 4,
      patient: "Yasmine Aly",
      time: "02:30 PM",
      service: "Dental Filling",
      status: "Confirmed",
      timestamp: 1430,
    },
    {
      id: 5,
      patient: "Tarek Fadel",
      time: "09:00 AM",
      service: "Extraction",
      status: "Pending",
      timestamp: 900,
    },
    {
      id: 6,
      patient: "Hoda Farouk",
      time: "04:00 PM",
      service: "Implant",
      status: "Cancelled",
      timestamp: 1600,
    },
  ]);

  // Filtering and Sorting States
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [dateFilter, setDateFilter] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [activeTab, setActiveTab] = useState("All");
  const [currentPage, setCurrentPage] = useState(1);

  // 1. Filter by Search Query
  let filtered = appointments.filter(
    (app) =>
      app.patient.toLowerCase().includes(searchQuery.toLowerCase()) ||
      app.service.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // 2. Filter by Active Tab
  if (activeTab !== "All") {
    if (activeTab === "Today") {
      filtered = filtered.filter((a) => a.timestamp < 1300);
    } else if (activeTab === "Upcoming") {
      filtered = filtered.filter((a) => ["Confirmed", "Pending"].includes(a.status));
    } else {
      filtered = filtered.filter((a) => a.status === activeTab);
    }
  }

  // 3. Filter by Status Dropdown
  if (statusFilter) {
    filtered = filtered.filter((a) => a.status === statusFilter);
  }

  // 4. Filter by Date Dropdown
  if (dateFilter) {
    if (dateFilter === "today") {
      filtered = filtered.filter((a) => a.timestamp < 1300);
    } else if (dateFilter === "upcoming") {
      filtered = filtered.filter((a) => ["Confirmed", "Pending"].includes(a.status));
    } else if (dateFilter === "week") {
      filtered = filtered.filter((a) => a.id % 2 === 0);
    }
  }

  // 5. Sorting by Time
  if (sortBy === "timeAsc") {
    filtered.sort((a, b) => a.timestamp - b.timestamp);
  } else if (sortBy === "timeDesc") {
    filtered.sort((a, b) => b.timestamp - a.timestamp);
  }

  // 6. Pagination Calculations
  const itemsPerPage = 2;
  const totalPages = Math.max(Math.ceil(filtered.length / itemsPerPage), 1);
  const activePage = Math.min(currentPage, totalPages);

  const paginatedData = filtered.slice(
    (activePage - 1) * itemsPerPage,
    activePage * itemsPerPage
  );

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
          onChange={(e) => {
            setSearchQuery(e.target.value);
            setCurrentPage(1);
          }}
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
            { value: "Pending", label: "Pending" },
            { value: "Confirmed", label: "Confirmed" },
            { value: "Completed", label: "Completed" },
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
            { value: "week", label: "This Week" },
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
            { value: "timeAsc", label: "Time Ascending" },
            { value: "timeDesc", label: "Time Descending" },
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

      <DataTable
        columns={columns}
        data={paginatedData}
        actions={(row) => (
          <div className="doctor-table-actions">
            <button onClick={() => navigate(`/doctor/appointments/${row.id}`)}>
              View
            </button>

            <button onClick={() => navigate(`/doctor/patients/${row.id}`)}>
              Record
            </button>

            <button onClick={() => navigate("/doctor/prescriptions")}>
              Prescription
            </button>
          </div>
        )}
      />

      <Pagination
        currentPage={activePage}
        totalPages={totalPages}
        onPageChange={(page) => setCurrentPage(page)}
      />
    </div>
  );
}

export default Appointments;
