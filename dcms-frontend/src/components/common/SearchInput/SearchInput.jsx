import "./SearchInput.css";

function SearchInput({ placeholder, value, onChange, onSearch }) {
  return (
    <div className="search-input">
      <span>🔍</span>

      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        onKeyDown={(event) => {
          if (event.key === "Enter") {
            onSearch();
          }
        }}
      />

      <button onClick={onSearch}>Search</button>
    </div>
  );
}

export default SearchInput;
