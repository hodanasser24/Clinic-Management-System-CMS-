import { useNavigate } from "react-router-dom";
import SearchInput from "../../../components/common/SearchInput/SearchInput";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import "./Patients.css";

function Patients() {
  const navigate = useNavigate();

  const columns = [
    { key: "name", label: "Patient" },
    { key: "age", label: "Age" },
    { key: "phone", label: "Phone" },
    { key: "lastVisit", label: "Last Visit" },
    { key: "status", label: "Status" },
  ];

  const data = [
    {
      id: 1,
      name: "Ahmed Ali",
      age: 24,
      phone: "01012345678",
      lastVisit: "05 Jul 2026",
      status: "Active",
    },
    {
      id: 2,
      name: "Mona Hassan",
      age: 29,
      phone: "01098765432",
      lastVisit: "04 Jul 2026",
      status: "Follow Up",
    },
    {
      id: 3,
      name: "Omar Mohamed",
      age: 31,
      phone: "01055555555",
      lastVisit: "02 Jul 2026",
      status: "Active",
    },
  ];

  return (
    <div className="doctor-patients-page">
      <div className="doctor-patients-header">
        <div>
          <h1>Doctor Patients</h1>
          <p>Review patients assigned to your appointments.</p>
        </div>
      </div>

      <div className="doctor-patients-toolbar">
        <SearchInput placeholder="Search patient..." />

        <FilterDropdown
          label="Status"
          value=""
          onChange={() => {}}
          options={[
            { value: "", label: "All Status" },
            { value: "active", label: "Active" },
            { value: "followup", label: "Follow Up" },
          ]}
        />

        <SortDropdown
          value=""
          onChange={() => {}}
          options={[
            { value: "newest", label: "Newest Visit" },
            { value: "oldest", label: "Oldest Visit" },
            { value: "az", label: "Name A-Z" },
          ]}
        />
      </div>

      <DataTable
        columns={columns}
        data={data}
        actions={(row) => (
          <div className="doctor-patient-actions">
            <button onClick={() => navigate(`/doctor/patients/${row.id}`)}>
              View
            </button>

            <button onClick={() => navigate("/doctor/medical-records")}>
              Records
            </button>

            <button onClick={() => navigate("/doctor/prescriptions")}>
              Prescription
            </button>
          </div>
        )}
      />

      <Pagination currentPage={1} totalPages={3} onPageChange={() => {}} />
    </div>
  );
}

export default Patients;
