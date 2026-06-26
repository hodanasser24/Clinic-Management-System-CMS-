import Button from "../common/Button";
import Input from "../common/Input";
import "./LoginForm.css";

function LoginForm({
  email,
  password,
  onEmailChange,
  onPasswordChange,
  onSubmit,
  loading,
  error,
}) {
  return (
    <form className="login-form" onSubmit={onSubmit}>
      {error && <div className="auth-error">{error}</div>}

      <Input
        label="Email Address"
        type="email"
        placeholder="Enter your email"
        value={email}
        onChange={onEmailChange}
      />

      <Input
        label="Password"
        type="password"
        placeholder="Enter your password"
        value={password}
        onChange={onPasswordChange}
      />

      <div className="login-options">
        <label>
          <input type="checkbox" />
          Remember me
        </label>

        <a href="/forgot-password">Forgot Password?</a>
      </div>

      <Button type="submit" disabled={loading}>
        {loading ? "Signing in..." : "Sign In →"}
      </Button>

      <p className="auth-switch">
        Don’t have an account? <a href="/register">Create account</a>
      </p>
    </form>
  );
}

export default LoginForm;
