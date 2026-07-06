import "./Reports.css";

function Reports() {
  return (
    <div className="reports-page">
      <div className="reports-header">
        <div>
          <h1>Reports</h1>
          <p>Clinic statistics and performance overview.</p>
        </div>

        <button>Export PDF</button>
      </div>

      <div className="report-cards">
        <div className="report-card">
          <h3>Today's Appointments</h3>
          <span>32</span>
        </div>

        <div className="report-card">
          <h3>Completed</h3>
          <span>24</span>
        </div>

        <div className="report-card">
          <h3>Cancelled</h3>
          <span>3</span>
        </div>

        <div className="report-card">
          <h3>Revenue</h3>
          <span>12,500 EGP</span>
        </div>
      </div>

      <div className="chart-placeholder">Charts will be connected later.</div>
    </div>
  );
}

export default Reports;
