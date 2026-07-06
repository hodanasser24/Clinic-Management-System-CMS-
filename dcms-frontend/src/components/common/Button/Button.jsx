import "./Button.css";

function Button({ children, type = "button", onClick, disabled }) {
  return (
    <button
      type={type}
      className="btn-primary"
      onClick={onClick}
      disabled={disabled}
    >
      {children}
    </button>
  );
}

export default Button;
