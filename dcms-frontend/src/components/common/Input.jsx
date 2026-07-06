import "./Input.css";

function Input({ label, type = "text", placeholder, value, onChange, name, ...rest }) {
  return (
    <div className="input-group">
      <label>{label}</label>
      <input
        name={name}
        type={type}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        {...rest}
      />
    </div>
  );
}

export default Input;
