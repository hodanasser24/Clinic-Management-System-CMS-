using Npgsql;
var connStr = "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.rxtqfshlwnfwlhensxuk;Password=Andata#sup2010;Maximum Pool Size=5";
await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();

// Check appointment 5
var cmd = new NpgsqlCommand(@"SELECT ""Id"", ""Status"", ""AttendanceStatus"", ""CompletedAt"" FROM ""Appointments"" WHERE ""Id"" = 5", conn);
await using (var r = await cmd.ExecuteReaderAsync()) {
    while (await r.ReadAsync())
        System.Console.WriteLine($"ID={r[0]} | Status={r[1]} | Attendance={r[2]} | CompletedAt={r[3]}");
}

// Fix: update appointment 5 to use string status if stored as string
var fixCmd = new NpgsqlCommand(@"UPDATE ""Appointments"" SET ""Status"" = 'Completed', ""AttendanceStatus"" = 'Attended' WHERE ""Id"" = 5 RETURNING ""Id"", ""Status""", conn);
await using (var r2 = await fixCmd.ExecuteReaderAsync()) {
    while (await r2.ReadAsync())
        System.Console.WriteLine($"Fixed: ID={r2[0]}, Status={r2[1]}");
}
