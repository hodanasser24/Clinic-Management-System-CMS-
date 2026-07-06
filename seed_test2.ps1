$ErrorActionPreference = "Stop"

try {
    # 1. Login Owner
    $ownerLogin = @{ Email = "owner@clinic.com"; Password = "Admin123!" } | ConvertTo-Json
    $ownerRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Auth/login" -Method POST -Body $ownerLogin -ContentType "application/json"
    $ownerToken = $ownerRes.accessToken
    $ownerHeaders = @{ Authorization = "Bearer $ownerToken" }
    Write-Output "Owner logged in."

    # 2. Create Admin
    try {
        $adminBody = @{ FullName = "Admin User"; Email = "admin@clinic.com"; Password = "Password123!"; Phone = "01000000001" } | ConvertTo-Json
        Invoke-RestMethod -Uri "http://localhost:5118/api/Owner/admins" -Method POST -Body $adminBody -ContentType "application/json" -Headers $ownerHeaders
        Write-Output "Admin created."
    } catch { Write-Output "Admin exists or failed" }

    # 3. Create Doctor
    try {
        $docBody = @{ FullName = "Doctor User"; Email = "doctor@clinic.com"; Password = "Password123!"; Phone = "01000000002"; Specialization = "Dentist"; Qualification = "MD"; ExperienceYears = 5 } | ConvertTo-Json
        Invoke-RestMethod -Uri "http://localhost:5118/api/Owner/doctors" -Method POST -Body $docBody -ContentType "application/json" -Headers $ownerHeaders
        Write-Output "Doctor created."
    } catch { Write-Output "Doctor exists or failed" }

    # 4. Login Patient
    $patientLogin = @{ Email = "ahmed.test@example.com"; Password = "Password123!" } | ConvertTo-Json
    $patientRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Auth/login" -Method POST -Body $patientLogin -ContentType "application/json"
    $patientToken = $patientRes.accessToken
    $patientHeaders = @{ Authorization = "Bearer $patientToken" }
    Write-Output "Patient logged in."

    # 5. Create Branch
    try {
        $branchBody = @{ Name = "Main Branch"; Location = "Cairo"; Phone = "123456" } | ConvertTo-Json
        $branchReq = Invoke-RestMethod -Uri "http://localhost:5118/api/Branch" -Method POST -Body $branchBody -ContentType "application/json" -Headers $ownerHeaders
        Invoke-RestMethod -Uri "http://localhost:5118/api/ModificationRequest/branches/$($branchReq.id)/decision" -Method PUT -Body '{"isApproved":true}' -ContentType "application/json" -Headers $ownerHeaders
        Write-Output "Branch created and approved."
    } catch { Write-Output "Branch error (might exist)" }

    # 6. Create Service
    try {
        $serviceBody = @{ Name = "Consultation"; Description = "General Checkup"; Price = 500 } | ConvertTo-Json
        $serviceReq = Invoke-RestMethod -Uri "http://localhost:5118/api/Service" -Method POST -Body $serviceBody -ContentType "application/json" -Headers $ownerHeaders
        Invoke-RestMethod -Uri "http://localhost:5118/api/ModificationRequest/services/$($serviceReq.id)/decision" -Method PUT -Body '{"isApproved":true}' -ContentType "application/json" -Headers $ownerHeaders
        Write-Output "Service created and approved."
    } catch { Write-Output "Service error (might exist)" }

    # Fetch IDs
    $branches = Invoke-RestMethod -Uri "http://localhost:5118/api/public/branches"
    $doctors = Invoke-RestMethod -Uri "http://localhost:5118/api/public/doctors"
    $services = Invoke-RestMethod -Uri "http://localhost:5118/api/public/services"

    $branchId = $branches[0].id
    $doctorId = $doctors[0].id
    $serviceId = $services[0].id

    # 7. Create Schedule
    try {
        $schedBody = @{ DoctorId = $doctorId; BranchId = $branchId; DayOfWeek = 1; StartTime = "09:00:00"; EndTime = "17:00:00"; SessionDurationMinutes = 30 } | ConvertTo-Json
        Invoke-RestMethod -Uri "http://localhost:5118/api/Schedule" -Method POST -Body $schedBody -ContentType "application/json" -Headers $ownerHeaders
        Write-Output "Schedule created."
    } catch { Write-Output "Schedule error (might exist)" }

    # 8. CRUD Appointment
    Write-Output "--- Starting Appointment CRUD ---"
    
    # Get first available slot on next Monday
    $monday = (Get-Date).AddDays(8)
    while ($monday.DayOfWeek -ne 'Monday') { $monday = $monday.AddDays(1) }

    $createApptBody = @{ PatientId = $patientRes.userId; DoctorId = $doctorId; BranchId = $branchId; ServiceId = $serviceId; Date = $monday.ToString("yyyy-MM-dd"); StartTime = "10:00:00"; Notes = "Tooth pain" } | ConvertTo-Json
    $createRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Appointment" -Method POST -Body $createApptBody -ContentType "application/json" -Headers $patientHeaders
    $appId = $createRes.id
    Write-Output "Appointment Created: $appId"

    $getRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Appointment/$appId" -Method GET -Headers $patientHeaders
    Write-Output "Appointment Read: $($getRes.date)"

    $rescheduleBody = @{ NewDate = $monday.AddDays(7).ToString("yyyy-MM-dd"); NewStartTime = "11:00:00" } | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5118/api/Appointment/$appId/reschedule" -Method PUT -Body $rescheduleBody -ContentType "application/json" -Headers $patientHeaders
    Write-Output "Appointment Rescheduled."

    $cancelBody = @{ Reason = "Changed mind" } | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5118/api/Appointment/$appId/cancel" -Method PUT -Body $cancelBody -ContentType "application/json" -Headers $patientHeaders
    Write-Output "Appointment Canceled."

    Write-Output "All Step 1 tests passed!"
} catch {
    Write-Output "ERROR: $($_.Exception.Message)"
    if ($_.ErrorDetails) {
        Write-Output $_.ErrorDetails.Message
    }
}
