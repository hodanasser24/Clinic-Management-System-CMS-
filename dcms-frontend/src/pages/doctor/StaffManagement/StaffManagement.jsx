import { useState, useEffect, useCallback } from "react";
import { Navigate } from "react-router-dom";
import DataTable from "../../../components/common/DataTable/DataTable";
import Pagination from "../../../components/common/Pagination/Pagination";
import { getAllAdmins, getAllDoctors, createAdmin, createDoctor, deactivateAccount, reactivateAccount } from "../../../services/ownerServices";
import "./StaffManagement.css";

function StaffManagement() {
  const role = localStorage.getItem("role");
  const isOwner = role === "Owner";

  const [activeTab, setActiveTab] = useState("Admins");
  const [staff, setStaff] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);

  // States
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  // Modal states
  const [showModal, setShowModal] = useState(false);
  const [modalType, setModalType] = useState(""); // "add_admin", "add_doctor", "deactivate", "reactivate"
  const [selectedStaff, setSelectedStaff] = useState(null);
  const [formData, setFormData] = useState({
    fullName: "", email: "", password: "", phone: "", specialization: "", qualification: "", experienceYears: "", bio: ""
  });

  const columns = activeTab === "Admins" 
    ? [
        { key: "id", label: "ID" },
        { key: "fullName", label: "Name" },
        { key: "email", label: "Email" },
        { key: "phone", label: "Phone" },
        { key: "status", label: "Status" },
      ]
    : [
        { key: "id", label: "ID" },
        { key: "fullName", label: "Name" },
        { key: "specialization", label: "Specialty" },
        { key: "email", label: "Email" },
        { key: "status", label: "Status" },
      ];

  const fetchStaff = useCallback(async () => {
    setLoading(true);
    try {
      if (activeTab === "Admins") {
        const response = await getAllAdmins(currentPage, itemsPerPage);
        setStaff(response.items || response.Items || []);
        setTotalCount(response.totalCount || 0);
      } else {
        const response = await getAllDoctors(currentPage, itemsPerPage);
        setStaff(response.items || response.Items || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err) {
      console.error("Failed to fetch staff", err);
    } finally {
      setLoading(false);
    }
  }, [activeTab, currentPage]);

  useEffect(() => {
    if (isOwner) {
      fetchStaff();
    }
  }, [fetchStaff, isOwner]);

  if (!isOwner) {
    return <Navigate to="/doctor/dashboard" replace />;
  }

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const openAddModal = () => {
    setModalType(activeTab === "Admins" ? "add_admin" : "add_doctor");
    setFormData({ fullName: "", email: "", password: "", phone: "", specialization: "", qualification: "", experienceYears: "", bio: "" });
    setShowModal(true);
  };

  const openStatusModal = (staffMember, action) => {
    setSelectedStaff(staffMember);
    setModalType(action); // "deactivate" or "reactivate"
    setFormData((prev) => ({ ...prev, reason: "" }));
    setShowModal(true);
  };

  const handleModalSubmit = async (e) => {
    e.preventDefault();
    try {
      if (modalType === "add_admin") {
        await createAdmin({
          fullName: formData.fullName,
          email: formData.email,
          password: formData.password,
          phone: formData.phone
        });
      } else if (modalType === "add_doctor") {
        await createDoctor({
          fullName: formData.fullName,
          email: formData.email,
          password: formData.password,
          phone: formData.phone,
          specialization: formData.specialization,
          qualification: formData.qualification,
          experienceYears: parseInt(formData.experienceYears || 0, 10),
          bio: formData.bio
        });
      } else if (modalType === "deactivate") {
        await deactivateAccount(selectedStaff.id, formData.reason || "Owner requested deactivation");
      } else if (modalType === "reactivate") {
        await reactivateAccount(selectedStaff.id);
      }
      setShowModal(false);
      fetchStaff();
    } catch (err) {
      alert(err.message || "Operation failed.");
    }
  };

  const totalPages = Math.max(Math.ceil(totalCount / itemsPerPage), 1);

  const mappedData = staff.map(s => ({
    ...s,
    status: s.isActive ? "Active" : "Inactive"
  }));

  return (
    <div className="staff-page">
      <div className="staff-header">
        <h1>Staff Management</h1>
        <p>Manage Admins and Doctors (Owner Only)</p>
      </div>

      <div className="staff-tabs">
        <button 
          className={activeTab === "Admins" ? "active" : ""} 
          onClick={() => { setActiveTab("Admins"); setCurrentPage(1); }}
        >
          Moderators (Admins)
        </button>
        <button 
          className={activeTab === "Doctors" ? "active" : ""} 
          onClick={() => { setActiveTab("Doctors"); setCurrentPage(1); }}
        >
          Doctors
        </button>
      </div>

      <div className="staff-toolbar">
        <button className="add-staff-btn" onClick={openAddModal}>
          + Add {activeTab === "Admins" ? "Admin" : "Doctor"}
        </button>
      </div>

      {loading ? (
        <p>Loading staff...</p>
      ) : (
        <DataTable
          columns={columns}
          data={mappedData}
          actions={(row) => (
            <div className="table-actions">
              {row.isActive ? (
                <button
                  className="danger"
                  onClick={() => openStatusModal(row, "deactivate")}
                >
                  Deactivate
                </button>
              ) : (
                <button
                  style={{ background: "#10b981", color: "white" }}
                  onClick={() => openStatusModal(row, "reactivate")}
                >
                  Reactivate
                </button>
              )}
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
        <div className="staff-modal-overlay">
          <div className="staff-modal">
            <h2>
              {modalType === "add_admin" && "Add New Admin"}
              {modalType === "add_doctor" && "Add New Doctor"}
              {modalType === "deactivate" && "Deactivate Account"}
              {modalType === "reactivate" && "Reactivate Account"}
            </h2>
            <form onSubmit={handleModalSubmit}>
              {(modalType === "add_admin" || modalType === "add_doctor") && (
                <>
                  <div className="form-group">
                    <label>Full Name</label>
                    <input type="text" name="fullName" value={formData.fullName} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Email</label>
                    <input type="email" name="email" value={formData.email} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Password</label>
                    <input type="password" name="password" value={formData.password} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Phone</label>
                    <input type="text" name="phone" value={formData.phone} onChange={handleInputChange} />
                  </div>
                </>
              )}

              {modalType === "add_doctor" && (
                <>
                  <div className="form-group">
                    <label>Specialization</label>
                    <input type="text" name="specialization" value={formData.specialization} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Qualification</label>
                    <input type="text" name="qualification" value={formData.qualification} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Experience (Years)</label>
                    <input type="number" name="experienceYears" value={formData.experienceYears} onChange={handleInputChange} required />
                  </div>
                  <div className="form-group">
                    <label>Bio</label>
                    <input type="text" name="bio" value={formData.bio} onChange={handleInputChange} />
                  </div>
                </>
              )}

              {modalType === "deactivate" && (
                <>
                  <p>Are you sure you want to deactivate {selectedStaff?.fullName}?</p>
                  <div className="form-group" style={{ marginTop: '15px' }}>
                    <label>Reason (Optional)</label>
                    <input type="text" name="reason" value={formData.reason} onChange={handleInputChange} />
                  </div>
                </>
              )}

              {modalType === "reactivate" && (
                <p>Are you sure you want to reactivate {selectedStaff?.fullName}?</p>
              )}
              
              <div className="staff-modal-actions">
                <button type="button" className="cancel-btn" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className={modalType === "deactivate" ? "delete-btn" : "save-btn"}>
                  Confirm
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default StaffManagement;
