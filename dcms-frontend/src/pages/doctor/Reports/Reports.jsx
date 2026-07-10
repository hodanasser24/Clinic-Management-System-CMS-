import "./Reports.css";

function Reports() {
  const reports = [
    {
      title: "Patients Treated Today",
      value: 12,
    },
    {
      title: "Completed Appointments",
      value: 9,
    },
    {
      title: "Pending Follow-ups",
      value: 4,
    },
    {
      title: "Prescriptions Issued",
      value: 7,
    },
  ];

  return (
    <div className="doctor-reports-page">
      <h1>Doctor Reports</h1>
      <p>Overview of your daily and weekly performance.</p>

      <div className="reports-grid">
        {reports.map((item) => (
          <div className="report-card" key={item.title}>
            <h3>{item.title}</h3>
            <span>{item.value}</span>
          </div>
        ))}
      </div>

      <div className="report-summary">
        <h2>Summary</h2>

        <p>
          This report summarizes your completed appointments, prescriptions and
          patient follow-ups.
        </p>
      </div>
    </div>
  );
}

export default Reports;
