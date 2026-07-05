import { BrowserRouter, Routes, Route } from "react-router-dom";
import AddAppointment from "../pages/moderator/AddAppointment/AddAppointment";
import EditAppointment from "../pages/moderator/EditAppointment/EditAppointment";

// Layouts
import MainLayout from "../layouts/MainLayout";
import DashboardLayout from "../layouts/DashboardLayout";

// Public Pages
import Home from "../pages/public/Home";

// Auth Pages
import Login from "../pages/auth/Login";
import Register from "../pages/auth/Register";
import ForgotPassword from "../pages/auth/ForgotPassword";
import ResetPassword from "../pages/auth/ResetPassword";

// Patient
import PatientDashboard from "../pages/patient/PatientDashboard";

// Moderator
import ModeratorDashboard from "../pages/moderator/Dashboard/Dashboard";
import Appointments from "../pages/moderator/Appointments/Appointments";
import AppointmentDetails from "../pages/moderator/AppointmentDetails/AppointmentDetails";
import Patients from "../pages/moderator/Patients/Patients";
import PatientDetails from "../pages/moderator/PatientDetails/PatientDetails";
import Reports from "../pages/moderator/Reports/Reports";
import Notifications from "../pages/moderator/Notifications/Notifications";
import Profile from "../pages/moderator/Profile/Profile";

function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public */}
        <Route
          path="/"
          element={
            <MainLayout>
              <Home />
            </MainLayout>
          }
        />

        {/* Authentication */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        <Route path="/reset-password" element={<ResetPassword />} />

        {/* Patient */}
        <Route
          path="/patient/dashboard"
          element={
            <DashboardLayout>
              <PatientDashboard />
            </DashboardLayout>
          }
        />

        {/* Moderator */}
        <Route
          path="/moderator/dashboard"
          element={
            <DashboardLayout>
              <ModeratorDashboard />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/appointments"
          element={
            <DashboardLayout>
              <Appointments />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/appointments/:id"
          element={
            <DashboardLayout>
              <AppointmentDetails />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/patients"
          element={
            <DashboardLayout>
              <Patients />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/patients/:id"
          element={
            <DashboardLayout>
              <PatientDetails />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/reports"
          element={
            <DashboardLayout>
              <Reports />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/notifications"
          element={
            <DashboardLayout>
              <Notifications />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/profile"
          element={
            <DashboardLayout>
              <Profile />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/appointments/add"
          element={
            <DashboardLayout>
              <AddAppointment />
            </DashboardLayout>
          }
        />

        <Route
          path="/moderator/appointments/edit/:id"
          element={
            <DashboardLayout>
              <EditAppointment />
            </DashboardLayout>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;
