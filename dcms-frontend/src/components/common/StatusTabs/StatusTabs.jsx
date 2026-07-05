import "./StatusTabs.css";

function StatusTabs({ tabs, activeTab, onChange }) {
  return (
    <div className="status-tabs">
      {tabs.map((tab) => (
        <button
          key={tab}
          className={activeTab === tab ? "active" : ""}
          onClick={() => onChange(tab)}
        >
          {tab}
        </button>
      ))}
    </div>
  );
}

export default StatusTabs;
