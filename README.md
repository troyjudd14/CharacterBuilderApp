# Character Builder App

A full-stack ASP.NET Core MVC app for creating and managing RPG characters, built with clean architecture patterns and tested business logic.

## Highlights

- CRUD workflow for characters: create, view, edit, delete
- Entity Framework Core with Azure SQL
- Layered design: Controller -> Service -> Repository
- Input models to prevent overposting
- Comprehensive unit tests for service and controller behavior
- Production-minded configuration with local secrets support

## Tech Stack

- .NET 9
- ASP.NET Core MVC
- Entity Framework Core
- Azure SQL Database
- xUnit, Moq, FluentAssertions

## Why This Project Matters

This project demonstrates practical backend engineering skills:

- Separation of concerns and dependency injection
- Domain-focused service layer with explicit result handling
- Testable architecture with mocked dependencies
- Secure configuration approach for public repositories

## Architecture Snapshot

- Controllers handle HTTP concerns and validation flow
- Services contain application and business logic
- Repositories encapsulate persistence operations
- EF Core manages schema migrations and data access

## Running Locally

1. Restore dependencies  
dotnet restore

2. Set local secret connection string  
dotnet user-secrets init  
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Your Azure SQL connection string"

3. Apply database migrations  
dotnet ef database update

4. Start the app  
dotnet run

5. Run tests  
dotnet test

## Security Notes

- No real credentials are committed
- Use local user secrets for development
- Use Azure App Service application settings in production

## Future Enhancements

- Authentication and user-specific character ownership
- CI/CD pipeline with automated test gates
- Observability and health checks
- Role-based admin features