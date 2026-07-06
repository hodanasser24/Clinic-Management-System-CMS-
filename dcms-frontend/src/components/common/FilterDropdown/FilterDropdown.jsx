import "./FilterDropdown.css";

function FilterDropdown({ label, value, options, onChange, ...rest }) {
  return (
    <div className="filter-dropdown">
      <label>{label}</label>

      <select value={value} onChange={(e) => onChange(e.target.value)} {...rest}>
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
