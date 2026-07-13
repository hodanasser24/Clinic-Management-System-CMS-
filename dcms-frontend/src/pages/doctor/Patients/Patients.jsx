import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import SearchInput from "../../../components/common/SearchInput/SearchInput";
import FilterDropdown from "../../../components/common/FilterDropdown/FilterDropdown";
import SortDropdown from "../../../components/common/SortDropdown/SortDropdown";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import apiClient from "../../../services/apiClient";
import "./Patients.css";

function Patients() {
  const navigate = useNavigate();

  const columns = [
    { key: "name", label: "Patient" },
    { key: "email", label: "Email" },
    { key: "phone", label: "Phone" },
    { key: "lastVisit", label: "Joined" },
    { key: "status", label: "Status" },
  ];

  const [patients, setPatients] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  const fetchPatients = useCallback(async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        page: currentPage,
        pageSize: itemsPerPage,
      });

      if (searchQuery) {
        if (/^\d+$/.test(searchQuery)) {
          params.append("PhoneNumber", searchQuery);
        } else {
          params.append("FullName", searchQuery);
        }
      }

      if (sortBy === "az") {
        params.append("SortBy", "FullName");
        params.append("SortDescending", "false");
      } else if (sortBy === "newest") {
        params.append("SortBy", "CreatedAt");
        params.append("SortDescending", "true");
      } else if (sortBy === "oldest") {
        params.append("SortBy", "CreatedAt");
        params.append("SortDescending", "false");
      }

      const response = await apiClient.get(`/api/patients/search?${params.toString()}`);
      
      const mappedData = (response.data.items || response.data.Items || []).map(p => ({
        id: p.id,
        name: p.fullName || "N/A",
        email: p.email || "N/A",
        phone: p.phone || "N/A",
        lastVisit: new Date(p.createdAt).toLocaleDateString(),
        status: p.isActive ? "Active" : "Inactive"
      }));

      setPatients(mappedData);
      setTotalCount(response.data.totalCount || 0);
    } catch (err) {
      console.error("Failed to fetch patients", err);
    } finally {
      setLoading(false);
    }
  }, [currentPage, searchQuery, sortBy]);

  useEffect(() => {
    fetchPatients();
  }, [fetchPatients]);

  const totalPages = Math.max(Math.ceil(totalCount / itemsPerPage), 1);

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
          placeholder="Search name or phone..."
          value={searchQuery}
          onChange={(e) => {
            setSearchQuery(e.target.value);
          }}
          onSearch={() => setCurrentPage(1)}
        />

        {/* Removed Status Filter dropdown as backend search doesn't support IsActive filter currently */}

        <SortDropdown
          value={sortBy}
          onChange={(val) => {
            setSortBy(val);
            setCurrentPage(1);
          }}
          options={[
            { value: "", label: "Sort By" },
            { value: "newest", label: "Newest Join" },
            { value: "oldest", label: "Oldest Join" },
            { value: "az", label: "Name A-Z" },
          ]}
        />
      </div>

      {loading ? (
        <p>Loading patients...</p>
      ) : (
        <DataTable
          columns={columns}
          data={patients}
          actions={(row) => (
            <div className="doctor-patient-actions">
              <button onClick={() => navigate(`/doctor/patients/${row.id}`)}>
                View
              </button>

              <button onClick={() => navigate(`/doctor/medical-records?patientId=${row.id}`)}>
                Records
              </button>

              <button onClick={() => navigate(`/doctor/prescriptions?patientId=${row.id}`)}>
                Prescription
              </button>
            </div>
          )}
        />
      )}

      {!loading && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={(page) => setCurrentPage(page)}
        />
      )}
    </div>
  );
}

export default Patients;
