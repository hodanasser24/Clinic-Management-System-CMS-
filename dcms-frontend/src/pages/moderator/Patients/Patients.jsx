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
    { key: "id", label: "ID" },
    { key: "name", label: "Patient" },
    { key: "phone", label: "Phone" },
    { key: "gender", label: "Gender" },
    { key: "age", label: "Age" },
  ];

  const [patients] = useState([
    {
      id: 1,
      name: "Ahmed Ali",
      phone: "01012345678",
      gender: "Male",
      age: 24,
    },
    {
      id: 2,
      name: "Mona Hassan",
      phone: "01098765432",
      gender: "Female",
      age: 29,
    },
  ]);

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [genderFilter, setGenderFilter] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // 1. Search Query Filter
  let filtered = patients.filter(
    (p) =>
      p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      p.phone.includes(searchQuery)
  );

  // 2. Gender Dropdown Filter
  if (genderFilter) {
    filtered = filtered.filter((p) => p.gender === genderFilter);
  }

  // 3. Sorting
  if (sortBy === "oldest") {
    filtered.sort((a, b) => a.id - b.id);
  } else if (sortBy === "newest") {
    filtered.sort((a, b) => b.id - a.id);
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
    <div className="patients-page">
      <div className="patients-header">
        <div>
          <h1>Patients</h1>
          <p>Manage all registered patients.</p>
        </div>

        <button
          className="add-patient-btn"
          onClick={() => alert("Add Patient will be connected later")}
        >
          + Add Patient
        </button>
      </div>

      <div className="patients-toolbar">
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
          label="Gender"
          value={genderFilter}
          onChange={(val) => {
            setGenderFilter(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "All" },
            { value: "Male", label: "Male" },
            { value: "Female", label: "Female" },
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
            { value: "newest", label: "Newest" },
            { value: "oldest", label: "Oldest" },
          ]}
        />
      </div>

      <DataTable
        columns={columns}
        data={paginatedData}
        actions={(row) => (
          <div className="table-actions">
            <button onClick={() => navigate(`/moderator/patients/${row.id}`)}>
              View
            </button>

            <button
              onClick={() => alert("Editing patient records in demo mode.")}
            >
              Edit
            </button>

            <button
              className="danger"
              onClick={() => alert("Patient record deleted.")}
            >
              Delete
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
