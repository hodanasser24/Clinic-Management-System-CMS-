warning: in the working copy of 'dcms-frontend/src/pages/patient/PatientAppointments/PatientAppointments.jsx', LF will be replaced by CRLF the next time Git touches it
[1mdiff --git a/dcms-frontend/src/pages/patient/PatientAppointments/PatientAppointments.jsx b/dcms-frontend/src/pages/patient/PatientAppointments/PatientAppointments.jsx[m
[1mindex 85bcb9c..8240c04 100644[m
[1m--- a/dcms-frontend/src/pages/patient/PatientAppointments/PatientAppointments.jsx[m
[1m+++ b/dcms-frontend/src/pages/patient/PatientAppointments/PatientAppointments.jsx[m
[36m@@ -14,6 +14,7 @@[m [mimport {[m
   getAvailableDates[m
 } from "../../../services/publicServices";[m
 import { formatTo12Hour } from "../../../utils/timeFormatter";[m
[32m+[m[32mimport { getFriendlyErrorMessage } from "../../../utils/errorMapper";[m
 import "./PatientAppointments.css";[m
 [m
 function PatientAppointments() {[m
[36m@@ -28,8 +29,11 @@[m [mfunction PatientAppointments() {[m
   const [doctors, setDoctors] = useState([]);[m
 [m
   // Filters & State[m
[31m-  const [searchQuery, setSearchQuery] = useState("");[m
[31m-  const [statusFilter, setStatusFilter] = useState("All");[m
[32m+[m[32m  const [doctorQuery, setDoctorQuery] = useState("");[m
[32m+[m[32m  const [dateQuery, setDateQuery] = useState("");[m
[32m+[m[32m  const [statusFilter, setStatusFilter] = useState("");[m
[32m+[m[32m  const [page, setPage] = useState(1);[m
[32m+[m[32m  const [totalPages, setTotalPages] = useState(1);[m
 [m
   // Booking Wizard State[m
   const [isBookingOpen, setIsBookingOpen] = useState(false);[m
[36m@@ -52,6 +56,7 @@[m [mfunction PatientAppointments() {[m
   const [isCancelOpen, setIsCancelOpen] = useState(false);[m
   const [cancelId, setCancelId] = useState(null);[m
   const [cancelReason, setCancelReason] = useState("");[m
[32m+[m[32m  const [cancelError, setCancelError] = useState("");[m
 [m
   // Details Modal State[m
   const [selectedAppt, setSelectedAppt] = useState(null);[m
[36m@@ -60,8 +65,32 @@[m [mfunction PatientAppointments() {[m
   const loadAppointments = async () => {[m
     setLoading(true);[m
     try {[m
[31m-      const res = await getPatientAppointments(userId);[m
[31m-      setAppointments(res?.items || res || []);[m
[32m+[m[32m      const params = {[m
[32m+[m[32m        page: page,[m
[32m+[m[32m        pageSize: 10,[m
[32m+[m[32m        sortDescending: true[m
[32m+[m[32m      };[m
[32m+[m[41m      [m
[32m+[m[32m      if (doctorQuery) params.doctorName = doctorQuery;[m
[32m+[m[32m      if (dateQuery) {[m
[32m+[m[32m        params.fromDate = dateQuery;[m
[32m+[m[32m        params.toDate = dateQuery;[m
[32m+[m[32m      }[m
[32m+[m[32m      if (statusFilter && statusFilter !== "All") {[m
[32m+[m[32m        if (statusFilter === "Upcoming") {[m
[32m+[m[32m            // Can't pass 'Upcoming' as status enum. We should just let backend handle status=Pending or Confirmed?[m
[32m+[m[32m            // Actually, backend has a specific endpoint for Upcoming.[m
[32m+[m[32m            // But we upgraded GetByPatient. We can just pass the enum string if it matches?[m
[32m+[m[32m            // Wait, we need to map "Upcoming" to Status properly, but backend doesn't support "Upcoming" in AppointmentQueryDto.Status.[m
[32m+[m[32m            // I'll change it to standard Statuses: Pending, Confirmed, Completed, Cancelled.[m
[32m+[m[32m        } else {[m
[32m+[m[32m            params.status = statusFilter;[m
[32m+[m[32m        }[m
[32m+[m[32m      }[m
[32m+[m
[32m+[m[32m      const res = await getPatientAppointments(userId, params);[m
[32m+[m[32m      setAppointments(res?.items || []);[m
[32m+[m[32m      setTotalPages(Math.ceil((res?.totalCount || 0) / 10));[m
     } catch (err) {[m
       console.error("Failed to load appointments:", err);[m
     } finally {[m
[36m@@ -71,6 +100,9 @@[m [mfunction PatientAppointments() {[m
 [m
   useEffect(() => {[m
     loadAppointments();[m
[32m+[m[32m  }, [userId, page, doctorQuery, dateQuery, statusFilter]);[m
[32m+[m
[32m+[m[32m  useEffect(() => {[m
     // Load lists for wizard[m
     getBranches().then(setBranches).catch(console.error);[m
     getServices().then(setServices).catch(console.error);[m
[36m@@ -157,33 +189,19 @@[m [mfunction PatientAppointments() {[m
 [m
   const handleCancelSubmit = async (e) => {[m
     e.preventDefault();[m
[32m+[m[32m    setCancelError("");[m
     try {[m
       await cancelAppointment(cancelId, cancelReason || "Cancelled by patient");[m
       setIsCancelOpen(false);[m
       setCancelReason("");[m
       loadAppointments();[m
     } catch (err) {[m
[31m-      alert(err.message || "Cancellation failed.");[m
[32m+[m[32m      setCancelError(getFriendlyErrorMessage(err));[m
     }[m
   };[m
 [m
[31m-  // Filtering[m
[31m-  let filtered = appointments.filter([m
[31m-    (app) =>[m
[31m-      app.doctorName?.toLowerCase().includes(searchQuery.toLowerCase()) ||[m
[31m-      app.serviceName?.toLowerCase().includes(searchQuery.toLowerCase()) ||[m
[31m-      app.branchName?.toLowerCase().includes(searchQuery.toLowerCase())[m
[31m-  );[m
[31m-[m
[31m-  if (statusFilter !== "All") {[m
[31m-    if (statusFilter === "Upcoming") {[m
[31m-      filtered = filtered.filter((app) =>[m
[31m-        ["Confirmed", "Pending"].includes(app.status)[m
[31m-      );[m
[31m-    } else {[m
[31m-      filtered = filtered.filter((app) => app.status === statusFilter);[m
[31m-    }[m
[31m-  }[m
[32m+[m[32m  // Filtering is now handled completely by the server[m
[32m+[m[32m  const filtered = appointments;[m
 [m
   return ([m
     <div className="patient-appointments-page">[m
[36m@@ -197,17 +215,24 @@[m [mfunction PatientAppointments() {[m
       <div className="toolbar">[m
         <input[m
           type="text"[m
[31m-          placeholder="Search doctor, service, branch..."[m
[31m-          value={searchQuery}[m
[31m-          onChange={(e) => setSearchQuery(e.target.value)}[m
[32m+[m[32m          placeholder="Search by Doctor Name..."[m
[32m+[m[32m          value={doctorQuery}[m
[32m+[m[32m          onChange={(e) => { setDoctorQuery(e.target.value); setPage(1); }}[m
[32m+[m[32m        />[m
[32m+[m
[32m+[m[32m        <input[m
[32m+[m[32m          type="date"[m
[32m+[m[32m          value={dateQuery}[m
[32m+[m[32m          onChange={(e) => { setDateQuery(e.target.value); setPage(1); }}[m
         />[m
 [m
         <select[m
           value={statusFilter}[m
[31m-          onChange={(e) => setStatusFilter(e.target.value)}[m
[32m+[m[32m          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}[m
         >[m
[31m-          <option value="All">All Statuses</option>[m
[31m-          <option value="Upcoming">Upcoming</option>[m
[32m+[m[32m          <option value="">All Statuses</option>[m
[32m+[m[32m          <option value="Pending">Pending</option>[m
[32m+[m[32m          <option value="Confirmed">Confirmed</option>[m
           <option value="Completed">Completed</option>[m
           <option value="Cancelled">Cancelled</option>[m
         </select>[m
[36m@@ -222,56 +247,73 @@[m [mfunction PatientAppointments() {[m
           No appointments found matching the filters.[m
         </div>[m
       ) : ([m
[31m-        <table className="appointments-table">[m
[31m-          <thead>[m
[31m-            <tr>[m
[31m-              <th>Doctor</th>[m
[31m-              <th>Service</th>[m
[31m-              <th>Branch</th>[m
[31m-              <th>Date</th>[m
[31m-              <th>Time</th>[m
[31m-              <th>Status</th>[m
[31m-              <th>Actions</th>[m
[31m-            </tr>[m
[31m-          </thead>[m
[31m-[m
[31m-          <tbody>[m
[31m-            {filtered.map((item) => ([m
[31m-              <tr key={item.id}>[m
[31m-                <td>Dr. {item.doctorName}</td>[m
[31m-                <td>{item.serviceName}</td>[m
[31m-                <td>