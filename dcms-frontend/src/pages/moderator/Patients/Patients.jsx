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
    { key: "id", label: "ID" },
    { key: "name", label: "Patient" },
    { key: "phone", label: "Phone" },
    { key: "gender", label: "Gender" },
    { key: "age", label: "Age" },
  ];

  const [patients, setPatients] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  
  // Modal states
  const [showModal, setShowModal] = useState(false);
  const [modalType, setModalType] = useState(""); // "add", "edit", "delete"
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [formData, setFormData] = useState({
    fullName: "", email: "", password: "", phone: "", dateOfBirth: "", medicalHistory: "", bloodType: "", gender: "", allergies: ""
  });

  // States
  const [searchQuery, setSearchQuery] = useState("");
  const [sortBy, setSortBy] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  const calculateAge = (dob) => {
    if (!dob) return "N/A";
    const diff = Date.now() - new Date(dob).getTime();
    return Math.abs(new Date(diff).getUTCFullYear() - 1970);
  };

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

      if (sortBy === "oldest") {
        params.append("SortBy", "CreatedAt");
        params.append("SortDescending", "false");
      } else if (sortBy === "newest") {
        params.append("SortBy", "CreatedAt");
        params.append("SortDescending", "true");
      }

      const response = await apiClient.get(`/api/patients/search?${params.toString()}`);
      
      const mappedData = (response.data.items || response.data.Items || []).map(p => ({
        id: p.id,
        name: p.fullName || "N/A",
        phone: p.phone || "N/A",
        gender: p.gender || "N/A", 
        age: calculateAge(p.dateOfBirth)
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

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const openAddModal = () => {
    setModalType("add");
    setFormData({ fullName: "", email: "", password: "", phone: "", dateOfBirth: "", medicalHistory: "", bloodType: "", gender: "", allergies: "" });
    setShowModal(true);
  };

  const openEditModal = async (patientId) => {
    try {
      const res = await apiClient.get(`/api/patients/${patientId}`);
      setSelectedPatient(res.data);
      setFormData({
        fullName: res.data.fullName || "",
        email: res.data.email || "", 
        phone: res.data.phone || "",
        dateOfBirth: res.data.dateOfBirth ? res.data.dateOfBirth.split('T')[0] : "",
        medicalHistory: res.data.medicalHistory || "",
        bloodType: res.data.bloodType || "",
        gender: res.data.gender || "",
        allergies: res.data.allergies || "",
      });
      setModalType("edit");
      setShowModal(true);
    } catch (e) {
      alert("Failed to fetch patient details");
    }
  };

  const openDeleteModal = (patient) => {
    setSelectedPatient(patient);
    setModalType("delete");
    setShowModal(true);
  };

  const handleModalSubmit = async (e) => {
    e.preventDefault();
    try {
      if (modalType === "add") {
        await apiClient.post("/api/patients", formData);
      } else if (modalType === "edit") {
        await apiClient.put(`/api/patients/${selectedPatient.id}`, formData);
      } else if (modalType === "delete") {
        await apiClient.delete(`/api/patients/${selectedPatient.id}`);
      }
      setShowModal(false);
      fetchPatients();
    } catch (err) {
      const data = err.response?.data;
      let msg = "Operation failed.";
      if (data) {
        if (data.errors) {
          msg = Object.values(data.errors).flat().join("\n");
        } else if (data.message) {
          msg = data.message;
        } else if (data.detail) {
          msg = data.detail;
        } else if (typeof data === "string") {
          msg = data;
        }
      } else if (err.message) {
        msg = err.message;
      }
      alert(msg);
    }
  };

  const totalPages = Math.max(Math.ceil(totalCount / itemsPerPage), 1);

  return (
    <div className="patients-page">
      <div className="patients-header">
        <div>
          <h1>Patients</h1>
          <p>Manage all registered patients.</p>
        </div>

        <button
          className="add-patient-btn"
          onClick={openAddModal}
        >
          + Add Patient
        </button>
      </div>

      <div className="patients-toolbar">
        <SearchInput
          placeholder="Search name or phone..."
          value={searchQuery}
          onChange={(e) => {
            setSearchQuery(e.target.value);
          }}
          onSearch={() => setCurrentPage(1)}
        />

        {/* Removed Gender Filter dropdown as backend search doesn't support Gender filter currently */}

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

      {loading ? (
        <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>Loading patients...</div>
      ) : (
        <DataTable
          columns={columns}
          data={patients}
          actions={(row) => (
            <div className="table-actions">
              <button onClick={() => navigate(`/moderator/patients/${row.id}`)}>
                View
              </button>

              {/* 
              <button
                onClick={() => openEditModal(row.id)}
              >
                Edit
              </button>
              */}

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

      {showModal && (
        <div className="patient-modal-overlay">
          <div className="patient-modal">
            <h2>
              {modalType === "add" ? "Add Patient" : modalType === "edit" ? "Edit Patient" : "Delete Patient"}
            </h2>
            <form onSubmit={handleModalSubmit}>
              {modalType !== "delete" ? (
                <>
                  <div className="form-group">
                    <label>Full Name</label>
                    <input type="text" name="fullName" value={formData.fullName} onChange={handleInputChange} required />
                  </div>
                  {modalType === "add" && (
                    <>
                      <div className="form-group">
                        <label>Email</label>
                        <input type="email" name="email" value={formData.email} onChange={handleInputChange} required />
                      </div>
                      <div className="form-group">
                        <label>Password</label>
                        <input type="password" name="password" value={formData.password} onChange={handleInputChange} required />
                      </div>
                    </>
                  )}
                  <div className="form-group">
                    <label>Phone</label>
                    <input type="text" name="phone" value={formData.phone} onChange={handleInputChange} />
                  </div>
                  <div className="form-group">
                    <label>Date of Birth</label>
                    <input type="date" name="dateOfBirth" value={formData.dateOfBirth} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Gender</label>
                    <select name="gender" value={formData.gender} onChange={handleInputChange}>
                      <option value="">Select Gender</option>
                      <option value="Male">Male</option>
                      <option value="Female">Female</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Blood Type</label>
                    <input type="text" name="bloodType" value={formData.bloodType} onChange={handleInputChange} />
                  </div>
                  <div className="form-group">
                    <label>Allergies</label>
                    <input type="text" name="allergies" value={formData.allergies} onChange={handleInputChange} />
                  </div>
                  <div className="form-group">
                    <label>Medical History</label>
                    <input type="text" name="medicalHistory" value={formData.medicalHistory} onChange={handleInputChange} />
                  </div>
                </>
              ) : (
                <p>Are you sure you want to deactivate patient {selectedPatient?.name}?</p>
              )}
              
              <div className="patient-modal-actions">
                <button type="button" className="cancel-btn" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className={modalType === "delete" ? "delete-btn" : "save-btn"}>
                  {modalType === "delete" ? "Confirm Delete" : "Save"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default Patients;
