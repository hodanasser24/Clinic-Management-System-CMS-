import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Appointments.css";

import SearchInput from "../../../components/common/SearchInput/SearchInput";
import StatusTabs from "../../../components/common/StatusTabs/StatusTabs";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";

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
    "Rescheduled",
  ];

  const columns = [
    { key: "id", label: "ID" },
    { key: "patient", label: "Patient" },
    { key: "doctor", label: "Doctor" },
    { key: "date", label: "Date" },
    { key: "status", label: "Status" },
  ];

  const [appointments] = useState([
    {
      id: 1,
      patient: "Ahmed Ali",
      doctor: "Dr. Sara",
      date: "05 Jul",
      status: "Pending",
      doctorId: "1",
    },
    {
      id: 2,
      patient: "Mona Hassan",
      doctor: "Dr. Omar",
      date: "05 Jul",
      status: "Completed",
      doctorId: "2",
    },
    {
      id: 3,
      patient: "Omar Mohamed",
      doctor: "Dr. Sara",
      date: "06 Jul",
      status: "Confirmed",
      doctorId: "1",
    },
    {
      id: 4,
      patient: "Yasmine Aly",
      doctor: "Dr. Omar",
      date: "07 Jul",
      status: "Cancelled",
      doctorId: "2",
    },
  ]);

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [doctorFilter, setDoctorFilter] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // 1. Search Query Filter
  let filtered = appointments.filter(
    (app) =>
      app.patient.toLowerCase().includes(searchQuery.toLowerCase()) ||
      app.doctor.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // 2. Active Tab Filter
  if (activeTab !== "All") {
    filtered = filtered.filter((app) => app.status === activeTab);
  }

  // 3. Doctor Dropdown Filter
  if (doctorFilter) {
    filtered = filtered.filter((app) => app.doctorId === doctorFilter);
  }

  // 4. Sorting
  if (sortBy === "oldest") {
    filtered.sort((a, b) => a.id - b.id);
  } else if (sortBy === "newest") {
    filtered.sort((a, b) => b.id - a.id);
  }

  // 5. Pagination
  const itemsPerPage = 2;
  const totalPages = Math.max(Math.ceil(filtered.length / itemsPerPage), 1);
  const activePage = Math.min(currentPage, totalPages);
  const paginatedData = filtered.slice(
    (activePage - 1) * itemsPerPage,
    activePage * itemsPerPage
  );

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
        <SearchInput
          placeholder="Search appointment..."
          value={searchQuery}
          onChange={(e) => {
            setSearchQuery(e.target.value);
            setCurrentPage(1);
          }}
          onSearch={() => setCurrentPage(1)}
        />

        <FilterDropdown
          label="Doctor"
          value={doctorFilter}
          onChange={(val) => {
            setDoctorFilter(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "All Doctors" },
            { value: "1", label: "Dr. Sara" },
            { value: "2", label: "Dr. Omar" },
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

      <DataTable
        columns={columns}
        data={paginatedData}
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

            <button
              className="danger"
              onClick={() => alert("Appointment has been cancelled.")}
            >
              Cancel
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
