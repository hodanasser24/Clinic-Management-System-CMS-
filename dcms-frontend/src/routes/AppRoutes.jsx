import { BrowserRouter, Routes, Route } from "react-router-dom";

// Layouts
import MainLayout from "../layouts/MainLayout";
import DashboardLayout from "../layouts/DashboardLayout";

// Public
import Home from "../pages/public/Home";

// Auth
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
import AddAppointment from "../pages/moderator/AddAppointment/AddAppointment";
import EditAppointment from "../pages/moderator/EditAppointment/EditAppointment";
import Patients from "../pages/moderator/Patients/Patients";
import PatientDetails from "../pages/moderator/PatientDetails/PatientDetails";
import Reports from "../pages/moderator/Reports/Reports";
import Notifications from "../pages/moderator/Notifications/Notifications";
import Profile from "../pages/moderator/Profile/Profile";

// Doctor
import DoctorDashboard from "../pages/doctor/Dashboard/Dashboard";
import DoctorAppointments from "../pages/doctor/Appointments/Appointments";
import DoctorAppointmentDetails from "../pages/doctor/AppointmentDetails/AppointmentDetails";
import DoctorPatients from "../pages/doctor/Patients/Patients";
import DoctorPatientDetails from "../pages/doctor/PatientDetails/PatientDetails";
import MedicalRecords from "../pages/doctor/MedicalRecords/MedicalRecords";
import Prescriptions from "../pages/doctor/Prescriptions/Prescriptions";
import DoctorReports from "../pages/doctor/Reports/Reports";
import DoctorNotifications from "../pages/doctor/Notifications/Notifications";
import DoctorProfile from "../pages/doctor/Profile/Profile";

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

        {/* Auth */}
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

        {/* Doctor */}
        <Route
          path="/doctor/dashboard"
          element={
            <DashboardLayout>
              <DoctorDashboard />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/appointments"
          element={
            <DashboardLayout>
              <DoctorAppointments />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/appointments/:id"
          element={
            <DashboardLayout>
              <DoctorAppointmentDetails />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/patients"
          element={
            <DashboardLayout>
              <DoctorPatients />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/patients/:id"
          element={
            <DashboardLayout>
              <DoctorPatientDetails />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/medical-records"
          element={
            <DashboardLayout>
              <MedicalRecords />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/prescriptions"
          element={
            <DashboardLayout>
              <Prescriptions />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/reports"
          element={
            <DashboardLayout>
              <DoctorReports />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/notifications"
          element={
            <DashboardLayout>
              <DoctorNotifications />
            </DashboardLayout>
          }
        />

        <Route
          path="/doctor/profile"
          element={
            <DashboardLayout>
              <DoctorProfile />
            </DashboardLayout>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;
