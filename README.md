# 🦷 DCMS – Dental Clinic Management System

A full-stack Clinic Management System developed to streamline dental clinic operations, including appointment management, patient records, prescriptions, reporting, notifications, and role-based access control.

---

# 📌 Project Overview

DCMS is a web-based application that digitizes the complete workflow of a dental clinic.

The system provides dedicated portals for:

- Patient
- Doctor
- Moderator (Receptionist)
- Admin
- Owner

Each role has its own dashboard and permissions.

---

# ✨ Main Features

## 👤 Authentication

- JWT Authentication
- Refresh Token
- Role-Based Authorization
- Change Password
- Forgot Password
- Secure API Protection

---

## 🩺 Patient Portal

- Register/Login
- Book Appointment
- Cancel Appointment
- View Appointment History
- View Prescriptions
- View Medical Records
- View Notifications
- Manage Profile

---

## 👨‍⚕️ Doctor Portal

- Daily Dashboard
- Today's Schedule
- Appointment Details
- Clinical Workspace
- Dental Chart
- Medical Reports
- Prescription Management
- Mark Attendance
- Urgent Cases

---

## 👩‍💼 Moderator Portal

- Manage Appointments
- Confirm / Reject Bookings
- Edit Appointments
- Reports Dashboard
- Patient Management
- Notifications

---

## 👑 Owner Portal

Owner inherits all Doctor capabilities and additionally can:

- Staff Management
- Dashboard Reports
- System Statistics

---

## ⚙️ Admin Portal

- Manage Users
- Manage Doctors
- Manage Branches
- Manage Services
- Manage FAQs
- Manage Offers
- Notifications
- Reports

---

# 🏗 Architecture

The backend follows **Clean Architecture**.

```
Presentation Layer
        │
Controllers (Web API)
        │
Application Layer
        │
Business Logic
        │
Domain Layer
        │
Infrastructure Layer
        │
SQL Server
```

Project Layers:

```
DCMS.WebAPI
DCMS.Application
DCMS.Domain
DCMS.Infrastructure
dcms-frontend
```

---

# 🧱 Design Patterns Used

- Clean Architecture
- Repository Pattern
- Unit of Work
- Dependency Injection
- DTO Pattern
- Service Layer
- JWT Authentication
- Role-Based Authorization

---

# 🛠 Technologies

## Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- LINQ
- JWT Authentication
- AutoMapper
- Dependency Injection

---

## Frontend

- React.js
- Vite
- React Router
- Axios
- CSS

---

## Database

- SQL Server

---

## Tools

- Visual Studio 2022
- VS Code
- Postman
- Swagger
- Git
- GitHub

---

# 📂 Project Structure

```
DCMS
│
├── DCMS.Application
├── DCMS.Domain
├── DCMS.Infrastructure
├── DCMS.WebAPI
├── dcms-frontend
├── SeedHelper
│
├── Tests
│   ├── Postman
│   ├── Swagger
│   ├── Scripts
│   └── E2E
│
└── README.md
```

---

# 🚀 Installation

## Clone Repository

```bash
git clone https://github.com/<your-repository>.git
```

---

## Backend

```bash
cd DCMS.WebAPI
```

Restore packages

```bash
dotnet restore
```

Run migrations

```bash
dotnet ef database update
```

Run API

```bash
dotnet run
```

API

```
https://localhost:7299
```

Swagger

```
https://localhost:7299/swagger
```

---

## Frontend

```bash
cd dcms-frontend
```

Install packages

```bash
npm install
```

Run

```bash
npm run dev
```

Frontend

```
http://localhost:5173
```

---


> Default passwords can be configured through the seed configuration.

---

# 📑 API Documentation

Swagger documentation is available after running the backend.

```
/swagger
```

Additional API testing files are available under:

```
Tests/
    Postman/
    Swagger/
```

---

# 🧪 Testing

The repository includes:

- Postman Collections
- Swagger JSON
- E2E Test Files
- Utility Scripts

Located under:

```
Tests/
```

---

# 👨‍💻 Team

Developed as a graduation project by the DCMS Team.

---

# 📄 License

This project was developed for educational purposes.
