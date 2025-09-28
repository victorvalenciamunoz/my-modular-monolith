# My Modular Monolith

Proyecto de aprendizaje que demuestra un Monolito Modular en .NET 10 (`net10.0`) aplicando DDD, principios SOLID y Clean Architecture. Expone una REST API, se orquesta con .NET Aspire y aplica patrones como Mediator, Result, Specification y Unit of Work. La cach� utiliza FusionCache (actualmente L1/en memoria).

> Nota: La soluci�n usa paquetes preview para .NET 10; las APIs pueden cambiar.

## M�dulos y proyectos

- `MyModularMonolith.Api` (`MyModularMonolith.Api.csproj`)
  - Proyecto de REST API (Swagger/OpenAPI, Serilog, integraci�n EF Core)
- `MyModularMonolith.AppHost` (`MyModularMonolith.AppHost.csproj`)
  - Aspire AppHost que orquesta la app y dependencias (SQL Server, MailDev)
- `MyModularMonolith.Modules.AI` (`MyModularMonolith.Modules.AI.csproj`)
  - Funcionalidades de IA usando `Microsoft.Extensions.AI` y `GeminiDotnet.Extensions.AI`
- `MyModularMonolith.Modules.Gyms` (`MyModularMonolith.Modules.Gyms.csproj`)
  - Dominio/l�gica de gimnasios, EF Core, Specifications, FusionCache (L1)
- `MyModularMonolith.Modules.Users` (`MyModularMonolith.Modules.Users.csproj`)
  - Dominio/l�gica de usuarios, Identity + JWT
- `MyModularMonolith.Shared`, `ServiceDefaults` y proyectos `*.Contracts`
  - Abstracciones transversales, contratos entre m�dulos y convenciones comunes

## Arquitectura y patrones

- Clean Architecture con l�mites expl�citos por m�dulo (Monolito Modular)
- DDD en cada m�dulo (entidades, value objects, specifications)
- Capa de aplicaci�n con `MediatR` (handlers para comandos/queries)
- Patr�n Result con `ErrorOr` para modelar �xito/fracaso
- Persistencia con EF Core (SQL Server). `DbContext` act�a como Unit of Work
- Cach� con `FusionCache` (L1) y serializaci�n `System.Text.Json`
- Observabilidad con Serilog (Console, OpenTelemetry) y paquetes de instrumentaci�n OTel
- IA con `Microsoft.Extensions.AI` + proveedor Gemini en el m�dulo de AI

## Tecnolog�as y paquetes clave

- API/Hosting: `Microsoft.AspNetCore.OpenApi`, `Serilog.*`, `Aspire.Microsoft.EntityFrameworkCore.SqlServer`
- Persistencia: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
- Aplicaci�n/DDD: `MediatR`, `ErrorOr`, `Ardalis.Specification.EntityFrameworkCore`
- Seguridad: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- Cach�: `ZiggyCreatures.FusionCache`, `ZiggyCreatures.FusionCache.Serialization.SystemTextJson`
- IA: `Microsoft.Extensions.AI`, `GeminiDotnet.Extensions.AI`
- Aspire/Recursos: `Aspire.Hosting.AppHost`, `Aspire.Hosting.SqlServer`, `BCat.Aspire.MailDev`

## Requisitos previos

- .NET 10 SDK (preview)
- Visual Studio 2022 (�ltima versi�n con soporte para .NET 10) o CLI de .NET
- Docker Desktop (recomendado para recursos Aspire como SQL Server y MailDev)

## C�mo ejecutar

Restaurar y compilar

- Visual Studio: Abrir la soluci�n, restaurar paquetes y compilar
- CLI:
  - `dotnet restore`
  - `dotnet build -c Debug`

Ejecutar con Aspire

- Visual Studio
  - Establecer `MyModularMonolith.AppHost` como proyecto de inicio
  - Iniciar depuraci�n (abrir� Aspire Dashboard y levantar� API + SQL Server + MailDev)
- CLI
  - `dotnet run --project ./MyModularMonolith.AppHost/MyModularMonolith.AppHost.csproj`

## API y documentaci�n

- Swagger/OpenAPI est� habilitado en `MyModularMonolith.Api`. Al ejecutar, abre la URL que muestra el Aspire Dashboard
- El repositorio incluye archivos `*.http` (p. ej. `MyModularMonolith.Api/Gyms.http`) para probar r�pidamente desde VS/VS Code

## Configuraci�n

- User Secrets
  - `MyModularMonolith.Api` y `MyModularMonolith.AppHost` definen `UserSecretsId`
  - Guarda valores sensibles (JWT, claves de IA, etc.) con User Secrets
- SQL Server
  - Suministrado por Aspire (`Aspire.Hosting.SqlServer`)
  - La cadena de conexi�n se inyecta/gestiona en tiempo de ejecuci�n por Aspire
- Logging/Tracing
  - Serilog con Console y OpenTelemetry sink
  - Puedes apuntar el exportador OTLP a tu collector (variables de entorno/User Secrets)

Ejemplo de secretos (solo ilustrativo):

```json
{
  "Jwt:Issuer": "https://localhost",
  "Jwt:Audience": "https://localhost",
  "Jwt:SigningKey": "super-secret-development-key",
  "AI:Gemini:ApiKey": "your-gemini-api-key"
}
```

## Roadmap

- A�adir `FusionCache` L2 (distribuida) y pol�ticas de cach� por caso de uso
- Ampliar casos de IA y configuraci�n de proveedores
- Endurecer observabilidad con pipeline OTel completo y dashboards
- M�s ejemplos end-to-end (Users/Gyms, eventos de dominio, tests de integraci�n)

---
Construido con `net10.0` y paquetes preview; sujeto a cambios en futuras versiones.