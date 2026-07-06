#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.0"
using Npgsql;

var connStr = "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.rxtqfshlwnfwlhensxuk;Password=Andata#sup2010;Maximum Pool Size=5";

using var conn = new NpgsqlConnection(connStr);
conn.Open();

// Insert a past completed appointment
var insertAppt = @"
INSERT INTO ""Appointments"" (""PatientId"", ""DoctorId"", ""BranchId"", ""ServiceId"", 
  ""Date"", ""StartTime"", ""EndTime"", ""Status"", ""AttendanceStatus"", ""IsUrgent"", 
  ""FollowUpFlag"", ""Notes"", ""CompletedAt"", ""CreatedAt"", ""UpdatedAt"") 
VALUES (18, 50, 1, 1, 
  '2026-06-01', '09:00:00', '09:30:00', 3, 1, false, 
  false, 'Past appointment for testing', '2026-06-01 09:30:00 UTC', 
  '2026-06-01 08:00:00 UTC', '2026-06-01 09:30:00 UTC')
RETURNING ""Id"";";
using var cmd = new NpgsqlCommand(insertAppt, conn);
var apptId = cmd.ExecuteScalar();
Console.WriteLine($"Created completed appointment ID: {apptId}");
