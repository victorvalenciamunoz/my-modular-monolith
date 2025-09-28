# My Modular Monolith

Learning project that demonstrates a Modular Monolith on .NET 10 (`net10.0`) applying DDD, SOLID principles, and Clean Architecture. It exposes a REST API, is orchestrated with .NET Aspire, and uses patterns such as Mediator, Result, Specification, and Unit of Work. Caching uses FusionCache (currently L1/in-memory).

> Note: The solution uses preview packages for .NET 10; APIs may change.

## Modules and Projects

- `MyModularMonolith.Api` (`MyModularMonolith.Api.csproj`)
  - REST API project (Swagger/OpenAPI, Serilog, EF Core integration)
- `MyModularMonolith.AppHost` (`MyModularMonolith.AppHost.csproj`)
  - Aspire AppHost orchestrating the app and dependencies (SQL Server, MailDev)
- `MyModularMonolith.Modules.AI` (`MyModularMonolith.Modules.AI.csproj`)
  - AI features using `Microsoft.Extensions.AI` and `GeminiDotnet.Extensions.AI`
- `MyModularMonolith.Modules.Gyms` (`MyModularMonolith.Modules.Gyms.csproj`)
  - Gyms domain/application logic, EF Core, Specifications, FusionCache (L1)
- `MyModularMonolith.Modules.Users` (`MyModularMonolith.Modules.Users.csproj`)
  - Users domain/application logic, Identity + JWT
- `MyModularMonolith.Shared`, `ServiceDefaults`, and `*.Contracts` projects
  - Cross-cutting abstractions, module contracts, and shared conventions

## Architecture and Patterns

- Clean Architecture with explicit per-module boundaries (Modular Monolith)
- DDD within each module (entities, value objects, specifications)
- Application layer with `MediatR` (handlers for commands/queries)
- Result pattern with `ErrorOr` to model success/failure
- Persistence with EF Core (SQL Server). `DbContext` plays the Unit of Work role
- Caching with `FusionCache` (L1) and `System.Text.Json` serialization
- Observability with Serilog (Console, OpenTelemetry) and OTel instrumentation packages
- AI with `Microsoft.Extensions.AI` + Gemini provider in the AI module

## Key Technologies and Packages

- API/Hosting: `Microsoft.AspNetCore.OpenApi`, `Serilog.*`, `Aspire.Microsoft.EntityFrameworkCore.SqlServer`
- Persistence: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
- Application/DDD: `MediatR`, `ErrorOr`, `Ardalis.Specification.EntityFrameworkCore`
- Security: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- Caching: `ZiggyCreatures.FusionCache`, `ZiggyCreatures.FusionCache.Serialization.SystemTextJson`
- AI: `Microsoft.Extensions.AI`, `GeminiDotnet.Extensions.AI`
- Aspire/Resources: `Aspire.Hosting.AppHost`, `Aspire.Hosting.SqlServer`, `BCat.Aspire.MailDev`

## Prerequisites

- .NET 10 SDK (preview)
- Visual Studio 2022 (latest with .NET 10 support) or .NET CLI
- Docker Desktop (recommended for Aspire resources like SQL Server and MailDev)

## How to Run

Restore and build

- Visual Studio: Open the solution, restore packages, and build
- CLI:
  - `dotnet restore`
  - `dotnet build -c Debug`

Run with Aspire

- Visual Studio
  - Set `MyModularMonolith.AppHost` as the Startup Project
  - Start debugging (this opens Aspire Dashboard and starts API + SQL Server + MailDev)
- CLI
  - `dotnet run --project ./MyModularMonolith.AppHost/MyModularMonolith.AppHost.csproj`

## API and Documentation

- Swagger/OpenAPI is enabled in `MyModularMonolith.Api`. When running, open the URL shown in Aspire Dashboard
- The repo includes `*.http` files (e.g., `MyModularMonolith.Api/Gyms.http`) for quick testing from VS/VS Code

## Configuration

- User Secrets
  - `MyModularMonolith.Api` and `MyModularMonolith.AppHost` define `UserSecretsId`
  - Store sensitive values (JWT, AI keys, etc.) with User Secrets
- SQL Server
  - Provided by Aspire (`Aspire.Hosting.SqlServer`)
  - Connection string is injected/managed by Aspire at runtime
- Logging/Tracing
  - Serilog with Console and OpenTelemetry sink
  - You can point the OTLP exporter to your collector (environment variables/User Secrets)

Sample secrets (illustrative only):

```json
{
  "Jwt:Issuer": "https://localhost",
  "Jwt:Audience": "https://localhost",
  "Jwt:SigningKey": "super-secret-development-key",
  "AI:Gemini:ApiKey": "your-gemini-api-key"
}
```

## Roadmap

- Add `FusionCache` L2 (distributed) and per-use-case cache policies
- Expand AI use cases and provider configuration
- Harden observability with a complete OTel pipeline and dashboards
- More end-to-end examples (Users/Gyms, domain events, integration tests)

---
Built with `net10.0` and preview packages; subject to change as the platform evolves.
