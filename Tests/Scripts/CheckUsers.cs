using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DCMS.Infrastructure.Data;
using DCMS.Domain.Entities;
using DCMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.rxtqfshlwnfwlhensxuk;Password=Andata#sup2010;Maximum Pool Size=20";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    
    var users = db.Users.ToList();
    Console.WriteLine($"Total users: {users.Count}");
    foreach(var u in users) {
        Console.WriteLine($"- {u.FullName} ({u.Email}) - Role: {u.Role}");
    }

    if (!users.Any(u => u.Role == UserRole.Owner))
    {
        var owner = new User
        {
            FullName = "Super Owner",
            Email = "owner@clinic.com",
            Role = UserRole.Owner,
            Phone = "01000000000",
            IsActive = true
        };
        owner.PasswordHash = hasher.HashPassword(owner, "Admin123!");
        db.Users.Add(owner);
        db.SaveChanges();
        Console.WriteLine("Added Owner: owner@clinic.com / Admin123!");
    }
}
