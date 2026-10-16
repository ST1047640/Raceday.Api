# RaceDay RESTful API

[![RaceDay API CI](https://github.com/ST1047640/Raceday.Api/actions/workflows/dotnet-ci.yml/badge.svg?branch=master)](https://github.com/ST1047640/Raceday.Api/actions/workflows/dotnet-ci.yml)

A robust, enterprise-grade RESTful Web API engineered using **ASP.NET Core (.NET 8)** and **Entity Framework Core**. The system manages athletic race events, race categories, participant profiles, and race enrolments with built-in validation, JWT role-based authorization, and automated CI/CD pipeline integration.

---

## 🚀 Key Features

- **Event & Category Management:** Complete CRUD endpoints for defining races, distances, entry limits, and schedules.
- **Participant Registration & Enrolment:** Secure registration handling duplicate checks, category capacity limits, and withdrawal states.
- **Role-Based Access Control:** Secure endpoints partitioned for `Admin`, `Organizer`, and `Participant` roles.
- **Data Integrity & Validations:** Fluent validations preventing conflicting race schedules and double-booking.
- **Automated CI/CD Pipeline:** GitHub Actions workflow executing on every push and pull request to validate builds and run test suites.
- **Automated Unit Testing:** Unit tests leveraging xUnit and InMemory database contexts testing critical business logic.

---

## 🛠️ Architecture & Tech Stack

- **Runtime & Framework:** .NET 8 (C# 12) / ASP.NET Core Web API
- **ORM / Database:** Entity Framework Core / Microsoft SQL Server & In-Memory DB (Testing)
- **API Documentation:** Swagger / OpenAPI (Swashbuckle)
- **Testing Framework:** xUnit, Moq, Microsoft.EntityFrameworkCore.InMemory
- **CI/CD Automation:** GitHub Actions (`ubuntu-latest`)

---

## 📂 Project Structure

```text
RaceDay.Api/
├── .github/
│   └── workflows/
│       └── dotnet-ci.yml        # CI/CD automated build and test pipeline
├── RaceDay.Api/                 # Core Web API Project
│   ├── Controllers/             # RESTful API Controllers
│   ├── Data/                    # AppDbContext, entity configurations, seeds
│   ├── Models/                  # Domain entities (User, Event, Category, Enrolment)
│   ├── DTOs/                    # Data Transfer Objects
│   ├── Program.cs               # Application bootstrapping and DI container
│   └── appsettings.json         # Configuration strings
├── RaceDay.Tests/               # Automated Unit Test Suite
│   └── UnitTest1.cs             # xUnit test cases covering enrolment logic
└── RaceDay.Api.sln              # Visual Studio Solution File
