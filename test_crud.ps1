$ErrorActionPreference = "Stop"

# 1. Register Patient
$registerBody = @{
    FullName = "Ahmed Test"
    Email = "ahmed.test@example.com"
    Password = "Password123!"
    Phone = "01012345678"
    DateOfBirth = "1990-01-01"
    MedicalHistory = "None"
} | ConvertTo-Json

Write-Output "--- Registering Patient ---"
try {
    $res = Invoke-WebRequest -Uri "http://localhost:5118/api/Auth/register" -Method POST -Body $registerBody -ContentType "application/json" -UseBasicParsing
    Write-Output "Register: $($res.StatusCode)"
} catch {
    Write-Output "Register: $($_.Exception.Response.StatusCode.value__) (Might already exist)"
}

# 2. Login
$loginBody = @{
    Email = "ahmed.test@example.com"
    Password = "Password123!"
} | ConvertTo-Json

Write-Output "--- Logging In ---"
try {
    $loginRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginRes.accessToken
    $patientId = $loginRes.userId
    Write-Output "Token received successfully! Patient ID: $patientId"
} catch {
    Write-Output "Login Failed: $($_.Exception.Message)"
    exit
}

$headers = @{
    Authorization = "Bearer $token"
}

# 3. Test Protected Endpoints
Write-Output "--- Testing Protected Endpoints ---"
try {
    $dashboard = Invoke-WebRequest -Uri "http://localhost:5118/api/Dashboard/summary" -Method GET -Headers $headers -UseBasicParsing
    Write-Output "Dashboard Summary: $($dashboard.StatusCode)"
} catch {
    Write-Output "Dashboard Summary (Patient Role): $($_.Exception.Response.StatusCode.value__)"
}

try {
    # Some endpoints might require Admin role, so testing `/api/Appointment` for list
    $appts = Invoke-WebRequest -Uri "http://localhost:5118/api/Appointment" -Method GET -Headers $headers -UseBasicParsing
    Write-Output "Appointments List: $($appts.StatusCode)"
} catch {
    Write-Output "Appointments List: $($_.Exception.Response.StatusCode.value__)"
}

# 4. Fetch Foreign Keys for Appointment
$branches = Invoke-RestMethod -Uri "http://localhost:5118/api/public/branches" -Method GET
$doctors = Invoke-RestMethod -Uri "http://localhost:5118/api/public/doctors" -Method GET
$services = Invoke-RestMethod -Uri "http://localhost:5118/api/public/services" -Method GET

if ($branches.Count -gt 0 -and $doctors.Count -gt 0 -and $services.Count -gt 0) {
    $branchId = $branches[0].id
    $doctorId = $doctors[0].id
    $serviceId = $services[0].id

    # 5. Create Appointment (POST)
    $createApptBody = @{
        PatientId = $patientId
        DoctorId = $doctorId
        BranchId = $branchId
        ServiceId = $serviceId
        Date = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
        StartTime = "10:00:00"
        Notes = "Test Appointment"
    } | ConvertTo-Json

    Write-Output "--- Creating Appointment ---"
    try {
        $createRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Appointment" -Method POST -Body $createApptBody -ContentType "application/json" -Headers $headers
        $appId = $createRes.id
        Write-Output "Appointment created with ID: $appId"

        # 6. Read Appointment (GET)
        Write-Output "--- Reading Appointment ---"
        $getRes = Invoke-RestMethod -Uri "http://localhost:5118/api/Appointment/$appId" -Method GET -Headers $headers
        Write-Output "Read Appointment: ID $($getRes.id) on $($getRes.date)"

        # 7. Update Appointment (Reschedule)
        Write-Output "--- Updating Appointment (Reschedule) ---"
        $rescheduleBody = @{
            NewDate = (Get-Date).AddDays(2).ToString("yyyy-MM-dd")
            NewStartTime = "11:00:00"
        } | ConvertTo-Json
        $putRes = Invoke-WebRequest -Uri "http://localhost:5118/api/Appointment/$appId/reschedule" -Method PUT -Body $rescheduleBody -ContentType "application/json" -Headers $headers -UseBasicParsing
        Write-Output "Reschedule: $($putRes.StatusCode)"

        # 8. Delete Appointment (Cancel)
        Write-Output "--- Canceling Appointment ---"
        $cancelBody = @{
            Reason = "No longer needed"
        } | ConvertTo-Json
        $cancelRes = Invoke-WebRequest -Uri "http://localhost:5118/api/Appointment/$appId/cancel" -Method PUT -Body $cancelBody -ContentType "application/json" -Headers $headers -UseBasicParsing
        Write-Output "Cancel: $($cancelRes.StatusCode)"
    } catch {
        Write-Output "CRUD Error: $($_.Exception.Message) - $($_.Exception.Response.StatusCode.value__)"
        if ($_.ErrorDetails) {
            Write-Output $_.ErrorDetails.Message
        }
    }
} else {
    Write-Output "Missing Branches, Doctors, or Services in DB to create appointment."
}
