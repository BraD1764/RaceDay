#  RaceDay - Event Management System

A full-stack web-based event management system for the South African road running, walking, and cycling community.

---

##  Project Overview

**RaceDay** is a platform that allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day using live weather information.

This project is built progressively across three parts:

| Part 1 | System Planning and Database |  Complete |
| Part 2 | RESTful API Development |  Complete |
| Part 3 | MVC Web Application |  Pending |

---

##  User Roles

###  Organiser
- Create, edit, and delete events
- Manage event categories (distances, entry fees, capacities)
- Capture participant results and rankings
- View all event enrolments

###  Participant
- Create an account and manage profile
- Browse upcoming events
- Enter events by selecting categories
- View personal enrolments and results

---

#  Project Structure

RaceDay/
├── docs/ # Part 1 Documentation
│ ├── erd.png # Entity Relationship Diagram
│ ├── api-endpoint-plan.md # API endpoint specifications
│ └── raceday-schema.sql # Database creation script
│
├── RaceDay.API/ # Part 2 - Web API Project
│ ├── Controllers/
│ │ ├── AuthController.cs
│ │ ├── UsersController.cs
│ │ ├── EventsController.cs
│ │ ├── CategoriesController.cs
│ │ ├── EnrolmentsController.cs
│ │ └── ResultsController.cs
│ ├── Program.cs
│ ├── appsettings.json
│ └── RaceDay.API.csproj
│
├── RaceDay.Core/ # Entities, DTOs, Interfaces
│ ├── Entities/
│ ├── DTOs/
│ ├── Interfaces/
│ └── RaceDay.Core.csproj
│
├── RaceDay.Infrastructure/ # DbContext & Services
│ ├── Data/
│ │ └── RaceDayDbContext.cs
│ ├── Services/
│ └── RaceDay.Infrastructure.csproj
│
├── RaceDay.Tests/ # Unit Tests
│ ├── AuthTests.cs
│ ├── EventTests.cs
│ ├── EnrolmentTests.cs
│ └── RaceDay.Tests.csproj
│
├── .github/workflows/ # CI/CD
│ └── dotnet.yml
│
├── RaceDay.slnx # Solution file
├── .gitignore
└── README.md


---

##  Technologies Used

| Backend | ASP.NET Core 8.0 Web API (C#) |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core (Code-First) |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit with In-Memory Database |
| CI/CD | GitHub Actions |

---

##  Setup Instructions

### Prerequisites

- Visual Studio 2022
- .NET 10.0 SDK
- Microsoft SQL Server (or SQL Server Express / LocalDB)
- Git
