import { useEffect, useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";

function ProtectedRoute({ children }) {
  const navigate = useNavigate();
  // Changed "accessToken" to "token" to match authServices.js
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem("token"));

  useEffect(() => {
    const checkAuth = () => {
      if (!localStorage.getItem("token")) {
        setIsAuthenticated(false);
        navigate("/login", { replace: true });
      }
    };

    // Revalidate when page is shown from BFCache (back/forward button)
    window.addEventListener("pageshow", checkAuth);
    
    // Also revalidate when storage changes (e.g. logout in another tab)
    window.addEventListener("storage", checkAuth);

    return () => {
      window.removeEventListener("pageshow", checkAuth);
      window.removeEventListener("storage", checkAuth);
    };
  }, [navigate]);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
}

export default ProtectedRoute;
