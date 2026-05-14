# MediTrack: Healthcare Appointment & Audit API

> **Learning-focused .NET/Azure project:**  
> MediTrack is a backend project I am building to get more comfortable with C#, ASP.NET Core Web API, Entity Framework Core, and Azure SQL Database.  
> My strongest background is in JavaScript/TypeScript full-stack development, so this project is my way of applying familiar backend concepts in the Microsoft/.NET ecosystem.

MediTrack is a healthcare-style appointment and audit API. The project focuses on patient records, provider records, appointment workflows, validation, audit logging, service-layer architecture, and cloud database integration.

This is a practical learning project where I am trying to build the kind of backend structure I would expect to see in a more serious enterprise or digital health environment.

---

## Current status

This project is in active development.

So far, the backend foundation is working:

- ASP.NET Core Web API backend
- Azure SQL Database integration
- Entity Framework Core with SQL Server provider
- EF Core migrations
- Patient, Provider, Appointment, and AuditLog models
- Patient, Provider, and Appointment API endpoints
- Appointment cancellation workflow
- ASP.NET Core Identity for user accounts
- JWT authentication
- Admin-only audit log access
- Audit logging for sensitive actions
- Authenticated user tracking in audit logs
- DTO-based request/response structure
- Basic validation using data annotations
- Service-layer architecture
- xUnit service tests
- GitHub Actions CI for build/test automation
- Deployed on Azure App Service


---

## Why I built this

I built this project to get hands-on practice with the .NET/Azure stack.

Most of my previous backend experience has been with Node.js, Express, TypeScript, and SQL databases. I wanted to build something similar in C#/.NET so I could better understand:

- How ASP.NET Core Web API projects are structured
- How controllers, services, DTOs, and models work together
- How Entity Framework Core handles database access
- How migrations create and update database schema
- How Azure SQL works with a real backend API
- How to design APIs with validation, auditability, and maintainability in mind

I chose a healthcare-style project because it naturally brings up important backend concerns like privacy, role-based access, audit logs, and reliable data handling.

---

## Project goal

The goal of MediTrack is to build a backend API where:

- Patients can be stored and retrieved
- Providers can be stored and retrieved
- Appointments can be created between patients and providers
- Sensitive actions can be recorded in audit logs
- The backend follows a clean controller/service/data-access structure
- The database runs on Azure SQL
- The project can eventually be tested, deployed, and monitored like a real cloud application

---

## Tech stack

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- Azure SQL Database
- SQL Server provider for EF Core
- Data annotations for request validation

### Database

- Azure SQL Database
- EF Core migrations
- SQL Server-style schema generation

### Testing

- xUnit
- Moq

### Cloud/DevOps

- Azure App Service
- GitHub Actions
- Application Insights

### Frontend

- React
- TypeScript

---

## Architecture

The backend follows a simple layered structure:

```txt
HTTP Request
    ↓
Controller
    ↓
Service
    ↓
Entity Framework Core / ApplicationDbContext
    ↓
Azure SQL Database
