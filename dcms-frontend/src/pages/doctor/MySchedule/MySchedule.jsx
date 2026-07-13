import { useEffect, useState } from "react";
import apiClient from "../../../services/apiClient";
import { getUserId } from "../../../services/authServices";
import { formatTo12Hour } from "../../../utils/timeFormatter";
import DataTable from "../../../components/common/DataTable/DataTable";
import "./MySchedule.css";

const daysOfWeek = [
  "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
];

function MySchedule() {
  const [schedules, setSchedules] = useState([]);
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  const [form, setForm] = useState({
    branchId: "",
    dayOfWeek: "1", // Monday
    startTime: "09:00",
    endTime: "17:00",
    sessionDurationMinutes: 30,
    breakDurationMinutes: 0
  });

  const doctorId = getUserId();

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");

      const branchesRes = await apiClient.get("/api/Public/branches");
      setBranches(branchesRes.data || []);

      if (doctorId) {
        const schedulesRes = await apiClient.get(`/api/Schedule/by-doctor/${doctorId}?pageSize=100`);
        setSchedules(schedulesRes.data?.items || []);
      }
    } catch (err) {
      setError(err.response?.data?.message || err.message || "Failed to load schedules.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [doctorId]);

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!form.branchId) {
      setError("Please select a branch.");
      return;
    }

    try {
      setSaving(true);
      setError("");
      await apiClient.post("/api/Schedule", {
        doctorId: doctorId,
        branchId: parseInt(form.branchId),
        dayOfWeek: parseInt(form.dayOfWeek),
        startTime: form.startTime,
        endTime: form.endTime,
        sessionDurationMinutes: parseInt(form.sessionDurationMinutes),
        breakDurationMinutes: parseInt(form.breakDurationMinutes) || 0
      });
      await loadData();
      // Reset form but keep defaults
      setForm({
        ...form,
        dayOfWeek: "1",
        startTime: "09:00",
        endTime: "17:00",
        sessionDurationMinutes: 30
      });
    } catch (err) {
      setError(err.response?.data?.message || err.message || "Failed to create schedule.");
    } finally {
      setSaving(false);
    }
  };

  const handleDeactivate = async (schedule) => {
    if (!window.confirm("Are you sure you want to deactivate this schedule?")) return;
    try {
      setError("");
      await apiClient.put(`/api/Schedule/${schedule.id}`, {
        startTime: schedule.startTime,
        endTime: schedule.endTime,
        sessionDurationMinutes: schedule.sessionDurationMinutes,
        isActive: false
      });
      await loadData();
    } catch (err) {
      setError(err.response?.data?.message || err.message || "Failed to deactivate schedule.");
    }
  };

  const handleReactivate = async (schedule) => {
    if (!window.confirm("Are you sure you want to reactivate this schedule?")) return;
    try {
      setError("");
      await apiClient.put(`/api/Schedule/${schedule.id}`, {
        startTime: schedule.startTime,
        endTime: schedule.endTime,
        sessionDurationMinutes: schedule.sessionDurationMinutes,
        isActive: true
      });
      await loadData();
    } catch (err) {
      setError(err.response?.data?.message || err.message || "Failed to reactivate schedule.");
    }
  };

  const getBranchName = (id) => {
    const b = branches.find(b => b.id === id);
    return b ? b.name : `Branch ${id}`;
  };

  const getDayName = (dayValue) => {
    return daysOfWeek[dayValue] || dayValue;
  };

  const columns = [
    { key: "branchName", label: "Branch" },
    { key: "day", label: "Day" },
    { key: "time", label: "Time" },
    { key: "slot", label: "Slot" },
    { key: "status", label: "Status" },
  ];

  const tableData = schedules.map(s => ({
    id: s.id,
    branchName: getBranchName(s.branchId),
    day: getDayName(s.dayOfWeek),
    time: `${formatTo12Hour(s.startTime)} - ${formatTo12Hour(s.endTime)}`,
    slot: `${s.sessionDurationMinutes} min`,
    isActive: s.isActive,
    status: (
      <span className={`status-badge ${s.isActive ? 'active' : 'inactive'}`}>
        {s.isActive ? 'Active' : 'Inactive'}
      </span>
    ),
    raw: s // keep original for actions
  }));

  if (loading) {
    return (
      <div className="schedule-page">
        <div className="schedule-header">
          <h1>My Schedule</h1>
          <p>Loading your schedules...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="schedule-page">
      <div className="schedule-header">
        <h1>My Schedule</h1>
        <p>Manage your weekly availability across different branches.</p>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="schedule-grid">
        <div className="schedule-card">
          <h2>Create Schedule</h2>
          <form className="schedule-form" onSubmit={handleCreate}>
            <div className="form-group">
              <label>Branch</label>
              <select 
                value={form.branchId} 
                onChange={(e) => setForm({ ...form, branchId: e.target.value })}
                required
              >
                <option value="">Select a branch</option>
                {branches.map(b => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>Day of Week</label>
              <select 
                value={form.dayOfWeek} 
                onChange={(e) => setForm({ ...form, dayOfWeek: e.target.value })}
                required
              >
                {daysOfWeek.map((day, index) => (
                  <option key={index} value={index}>{day}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>Start Time</label>
              <input 
                type="time" 
                value={form.startTime} 
                onChange={(e) => setForm({ ...form, startTime: e.target.value })}
                required
              />
            </div>

            <div className="form-group">
              <label>End Time</label>
              <input 
                type="time" 
                value={form.endTime} 
                onChange={(e) => setForm({ ...form, endTime: e.target.value })}
                required
              />
            </div>

            <div className="form-group">
              <label>Slot Duration (min)</label>
              <select 
                value={form.sessionDurationMinutes} 
                onChange={(e) => setForm({ ...form, sessionDurationMinutes: e.target.value })}
                required
              >
                <option value="15">15 minutes</option>
                <option value="30">30 minutes</option>
                <option value="45">45 minutes</option>
                <option value="60">60 minutes</option>
              </select>
            </div>

            <div className="form-group" style={{ justifyContent: "flex-end" }}>
              <button type="submit" className="btn-submit" disabled={saving}>
                {saving ? "Saving..." : "Add Schedule"}
              </button>
            </div>
          </form>
        </div>

        <div className="schedule-card">
          <h2>Existing Schedules</h2>
          {schedules.length === 0 ? (
            <p style={{ color: "#94a3b8" }}>No schedules found.</p>
          ) : (
            <DataTable
              columns={columns}
              data={tableData}
              actions={(row) => (
                <div className="table-actions">
                  {row.isActive ? (
                    <button 
                      className="danger"
                      onClick={() => handleDeactivate(row.raw)}
                    >
                      Deactivate
                    </button>
                  ) : (
                    <button 
                      className="success"
                      onClick={() => handleReactivate(row.raw)}
                    >
                      Reactivate
                    </button>
                  )}
                </div>
              )}
            />
          )}
        </div>
      </div>
    </div>
  );
}

export default MySchedule;
