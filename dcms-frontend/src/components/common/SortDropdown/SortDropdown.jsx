import "./SortDropdown.css";

function SortDropdown({ value, onChange, options }) {
  return (
    <div className="sort-dropdown">
      <label>Sort By</label>

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

export default SortDropdown;
