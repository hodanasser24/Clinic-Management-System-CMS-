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

  const data = [
    {
      id: 1,
      patient: "Ahmed Ali",
      doctor: "Dr. Sara",
      date: "05 Jul",
      status: "Pending",
    },
    {
      id: 2,
      patient: "Mona Hassan",
      doctor: "Dr. Omar",
      date: "05 Jul",
      status: "Completed",
    },
  ];

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
        <SearchInput placeholder="Search appointment..." />

        <FilterDropdown
          label="Doctor"
          value=""
          onChange={() => {}}
          options={[
            { value: "", label: "All Doctors" },
            { value: "1", label: "Dr. Sara" },
            { value: "2", label: "Dr. Omar" },
          ]}
        />

        <SortDropdown
          value=""
          onChange={() => {}}
          options={[
            { value: "newest", label: "Newest Booking" },
            { value: "oldest", label: "Oldest Booking" },
          ]}
        />
      </div>

      <StatusTabs tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />

      <DataTable
        columns={columns}
        data={data}
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

            <button className="danger">Cancel</button>
          </div>
        )}
      />

      <Pagination currentPage={1} totalPages={5} onPageChange={() => {}} />
    </div>
  );
}

export default Appointments;
