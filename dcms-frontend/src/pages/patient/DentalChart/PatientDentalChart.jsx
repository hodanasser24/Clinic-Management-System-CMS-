import { useState, useEffect, useCallback } from "react";
import "./PatientDentalChart.css";

import { getPatientDentalChart } from "../../../services/dentalChartServices";
import { getCurrentPatientId } from "../../../services/authServices";

import Loading from "../../../components/common/Loading/Loading";
import EmptyState from "../../../components/common/EmptyState/EmptyState";

// Enums mapping fallback if backend sends numbers
const TOOTH_STATUS_MAP = {
  0: "Healthy",
  1: "Decayed",
  2: "Filled",
  3: "Missing",
  4: "RootCanalTreated",
  5: "CrownPlaced",
  6: "NeedsExtraction",
  7: "UnderObservation"
};

const TREATMENT_TYPE_MAP = {
  0: "Cleaning",
  1: "Filling",
  2: "Extraction",
  3: "RootCanal",
  4: "Crown",
  5: "Bridge",
  6: "Implant",
  7: "Whitening",
  8: "Orthodontic",
  9: "Consultation"
};

function formatEnum(val, map) {
  if (val === null || val === undefined) return "None";
  if (typeof val === "number") return map[val] || val;
  // If string, add spaces before uppercase letters
  return val.replace(/([A-Z])/g, ' $1').trim();
}

function PatientDentalChart() {
  const [chartData, setChartData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  const [selectedTooth, setSelectedTooth] = useState(null); // stores the tooth number

  const loadChart = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const patientId = getCurrentPatientId();
      const data = await getPatientDentalChart(patientId);
      setChartData(data);
    } catch (err) {
      console.error("Failed to load dental chart", err);
      setError("Failed to load your dental chart. Please try again later.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadChart();
  }, [loadChart]);

  if (loading) return <div className="dental-chart-page"><Loading /></div>;
  if (error) return <div className="dental-chart-page"><EmptyState title="Error" message={error} /></div>;

  const records = chartData?.toothRecords || [];
  
  // Helper to find record for a specific tooth
  const getRecordForTooth = (num) => records.find(r => r.toothNumber === num);
  
  // Generate 1-16 (Upper) and 17-32 (Lower) arrays
  const upperTeeth = Array.from({ length: 16 }, (_, i) => i + 1);
  const lowerTeeth = Array.from({ length: 16 }, (_, i) => i + 17);

  const selectedRecord = selectedTooth ? getRecordForTooth(selectedTooth) : null;

  return (
    <div className="dental-chart-page">
      <div className="dental-chart-header">
        <div>
          <h1>My Dental Chart</h1>
          <p>
            Last Updated: {chartData?.lastUpdated ? new Date(chartData.lastUpdated).toLocaleDateString() : "Never"}
          </p>
        </div>
      </div>

      {chartData?.notes && (
        <div className="general-notes">
          <h3>Doctor's General Notes</h3>
          <p>{chartData.notes}</p>
        </div>
      )}

      <div className="dental-chart-content">
        {/* Interactive Grid */}
        <div className="chart-visual-container">
          <h3 style={{ margin: "0 0 20px 0", color: "#f8fafc", fontSize: "16px", textAlign: "center" }}>
            Upper Teeth
          </h3>
          <div className="teeth-row">
            {upperTeeth.map(num => {
              const rec = getRecordForTooth(num);
              const statusStr = rec ? (typeof rec.toothStatus === "number" ? TOOTH_STATUS_MAP[rec.toothStatus] : rec.toothStatus) : "Healthy";
              return (
                <div 
                  key={num} 
                  className={`tooth-box ${selectedTooth === num ? "selected" : ""}`}
                  data-status={statusStr}
                  onClick={() => setSelectedTooth(num)}
                  title={`Tooth ${num} - ${formatEnum(rec?.toothStatus || 0, TOOTH_STATUS_MAP)}`}
                >
                  {num}
                </div>
              );
            })}
          </div>

          <h3 style={{ margin: "20px 0", color: "#f8fafc", fontSize: "16px", textAlign: "center" }}>
            Lower Teeth
          </h3>
          <div className="teeth-row">
            {lowerTeeth.map(num => {
              const rec = getRecordForTooth(num);
              const statusStr = rec ? (typeof rec.toothStatus === "number" ? TOOTH_STATUS_MAP[rec.toothStatus] : rec.toothStatus) : "Healthy";
              return (
                <div 
                  key={num} 
                  className={`tooth-box ${selectedTooth === num ? "selected" : ""}`}
                  data-status={statusStr}
                  onClick={() => setSelectedTooth(num)}
                  title={`Tooth ${num} - ${formatEnum(rec?.toothStatus || 0, TOOTH_STATUS_MAP)}`}
                >
                  {num}
                </div>
              );
            })}
          </div>

          {/* Legend */}
          <div className="chart-legend">
            <div className="legend-item"><div className="legend-color" style={{ background: "#10b981" }}></div> Healthy</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#ef4444" }}></div> Decayed</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#3b82f6" }}></div> Filled</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#64748b" }}></div> Missing</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#8b5cf6" }}></div> Root Canal</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#f59e0b" }}></div> Crown</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#dc2626" }}></div> Needs Extraction</div>
            <div className="legend-item"><div className="legend-color" style={{ background: "#eab308" }}></div> Observation</div>
          </div>
        </div>

        {/* Selected Tooth Details Panel */}
        <div className="details-panel">
          <h3>Tooth Details</h3>
          
          {!selectedTooth ? (
            <div className="placeholder-text">
              Select a tooth from the chart to view its detailed records.
            </div>
          ) : (
            <div className="selected-tooth-info">
              <div className="detail-row">
                <div className="detail-label">Tooth Number</div>
                <div className="detail-value" style={{ fontSize: "24px", color: "#60a5fa" }}>
                  #{selectedTooth}
                </div>
              </div>

              <div className="detail-row">
                <div className="detail-label">Current Status</div>
                <div className="detail-value">
                  {formatEnum(selectedRecord?.toothStatus || 0, TOOTH_STATUS_MAP)}
                </div>
              </div>

              {selectedRecord && selectedRecord.treatmentType !== null && selectedRecord.treatmentType !== undefined && (
                <div className="detail-row">
                  <div className="detail-label">Latest Treatment</div>
                  <div className="detail-value">
                    {formatEnum(selectedRecord.treatmentType, TREATMENT_TYPE_MAP)}
                  </div>
                </div>
              )}

              {selectedRecord?.treatmentDate && (
                <div className="detail-row">
                  <div className="detail-label">Treatment Date</div>
                  <div className="detail-value">
                    {new Date(selectedRecord.treatmentDate).toLocaleDateString()}
                  </div>
                </div>
              )}

              {selectedRecord?.notes && (
                <div className="detail-row">
                  <div className="detail-label">Doctor Notes</div>
                  <div className="detail-value" style={{ fontStyle: "italic", lineHeight: "1.5" }}>
                    "{selectedRecord.notes}"
                  </div>
                </div>
              )}

              {!selectedRecord && (
                <div className="detail-row" style={{ marginTop: "20px" }}>
                  <p style={{ color: "#94a3b8", fontSize: "14px" }}>
                    No specific treatment records found for this tooth.
                  </p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default PatientDentalChart;
