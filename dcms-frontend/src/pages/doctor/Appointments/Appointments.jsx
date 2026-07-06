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

  const data = [
    {
      id: 1,
      patient: "Ahmed Ali",
      time: "10:00 AM",
      service: "Teeth Cleaning",
      status: "Pending",
    },
    {
      id: 2,
      patient: "Mona Hassan",
      time: "11:30 AM",
      service: "Root Canal",
      status: "Confirmed",
    },
    {
      id: 3,
      patient: "Omar Mohamed",
      time: "01:00 PM",
      service: "Consultation",
      status: "Completed",
    },
  ];

  return (
    <div className="doctor-appointments-page">
      <div className="doctor-appointments-header">
        <div>
          <h1>Doctor Appointments</h1>
          <p>Review today’s appointments and patient visits.</p>
        </div>
      </div>

      <div className="doctor-appointments-toolbar">
        <SearchInput placeholder="Search patient..." />

        <FilterDropdown
          label="Status"
          value=""
          onChange={() => {}}
          options={[
            { value: "", label: "All Status" },
            { value: "Pending", label: "Pending" },
            { value: "Confirmed", label: "Confirmed" },
            { value: "Completed", label: "Completed" },
          ]}
        />

        <FilterDropdown
          label="Date"
          value=""
          onChange={() => {}}
          options={[
            { value: "", label: "Today" },
            { value: "upcoming", label: "Upcoming" },
            { value: "week", label: "This Week" },
          ]}
        />

        <SortDropdown
          value=""
          onChange={() => {}}
          options={[
            { value: "timeAsc", label: "Time Ascending" },
            { value: "timeDesc", label: "Time Descending" },
          ]}
        />
      </div>

      <StatusTabs tabs={tabs} activeTab="All" onChange={() => {}} />

      <DataTable
        columns={columns}
        data={data}
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

      <Pagination currentPage={1} totalPages={4} onPageChange={() => {}} />
    </div>
  );
}

export default Appointments;
