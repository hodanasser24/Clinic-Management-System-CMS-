import "./FilterDropdown.css";

function FilterDropdown({ label, value, options, onChange }) {
  return (
    <div className="filter-dropdown">
      <label>{label}</label>

      <select value={value} onChange={(e) => onChange(e.target.value)}>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
}

export default FilterDropdown;
