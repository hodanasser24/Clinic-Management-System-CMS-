import { useState } from "react";
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

  const [patients] = useState([
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
  ]);

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // 1. Search Query Filter
  let filtered = patients.filter(
    (p) =>
      p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      p.phone.includes(searchQuery)
  );

  // 2. Status Dropdown Filter
  if (statusFilter) {
    filtered = filtered.filter((p) => {
      const matchVal = statusFilter === "followup" ? "Follow Up" : "Active";
      return p.status === matchVal;
    });
  }

  // 3. Sorting
  if (sortBy === "az") {
    filtered.sort((a, b) => a.name.localeCompare(b.name));
  } else if (sortBy === "newest") {
    filtered.sort((a, b) => b.id - a.id);
  } else if (sortBy === "oldest") {
    filtered.sort((a, b) => a.id - b.id);
  }

  // 4. Pagination
  const itemsPerPage = 2;
  const totalPages = Math.max(Math.ceil(filtered.length / itemsPerPage), 1);
  const activePage = Math.min(currentPage, totalPages);
  const paginatedData = filtered.slice(
    (activePage - 1) * itemsPerPage,
    activePage * itemsPerPage
  );

  return (
    <div className="doctor-patients-page">
      <div className="doctor-patients-header">
        <div>
          <h1>Doctor Patients</h1>
          <p>Review patients assigned to your appointments.</p>
        </div>
      </div>

      <div className="doctor-patients-toolbar">
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
            { value: "active", label: "Active" },
            { value: "followup", label: "Follow Up" },
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
            { value: "newest", label: "Newest Visit" },
            { value: "oldest", label: "Oldest Visit" },
            { value: "az", label: "Name A-Z" },
          ]}
        />
      </div>

      <DataTable
        columns={columns}
        data={paginatedData}
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

      <Pagination
        currentPage={activePage}
        totalPages={totalPages}
        onPageChange={(page) => setCurrentPage(page)}
      />
    </div>
  );
}

export default Patients;
