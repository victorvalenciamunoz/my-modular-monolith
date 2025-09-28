# My Modular Monolith

Proyecto de aprendizaje que demuestra un Monolito Modular en .NET 10 (`net10.0`) aplicando DDD, principios SOLID y Clean Architecture. Expone una REST API, se orquesta con .NET Aspire y aplica patrones como Mediator, Result, Specification y Unit of Work. La caché utiliza FusionCache (actualmente L1/en memoria).

> Nota: La solución usa paquetes preview para .NET 10; las APIs pueden cambiar.

## Módulos y proyectos

- `MyModularMonolith.Api` (`MyModularMonolith.Api.csproj`)
  - Proyecto de REST API (Swagger/OpenAPI, Serilog, integración EF Core)
- `MyModularMonolith.AppHost` (`MyModularMonolith.AppHost.csproj`)
  - Aspire AppHost que orquesta la app y dependencias (SQL Server, MailDev)
- `MyModularMonolith.Modules.AI` (`MyModularMonolith.Modules.AI.csproj`)
  - Funcionalidades de IA usando `Microsoft.Extensions.AI` y `GeminiDotnet.Extensions.AI`
- `MyModularMonolith.Modules.Gyms` (`MyModularMonolith.Modules.Gyms.csproj`)
  - Dominio/lógica de gimnasios, EF Core, Specifications, FusionCache (L1)
- `MyModularMonolith.Modules.Users` (`MyModularMonolith.Modules.Users.csproj`)
  - Dominio/lógica de usuarios, Identity + JWT
- `MyModularMonolith.Shared`, `ServiceDefaults` y proyectos `*.Contracts`
  - Abstracciones transversales, contratos entre módulos y convenciones comunes

## Arquitectura y patrones

- Clean Architecture con límites explícitos por módulo (Monolito Modular)
- DDD en cada módulo (entidades, value objects, specifications)
- Capa de aplicación con `MediatR` (handlers para comandos/queries)
- Patrón Result con `ErrorOr` para modelar éxito/fracaso
- Persistencia con EF Core (SQL Server). `DbContext` actúa como Unit of Work
- Caché con `FusionCache` (L1) y serialización `System.Text.Json`
- Observabilidad con Serilog (Console, OpenTelemetry) y paquetes de instrumentación OTel
- IA con `Microsoft.Extensions.AI` + proveedor Gemini en el módulo de AI

## Tecnologías y paquetes clave

- API/Hosting: `Microsoft.AspNetCore.OpenApi`, `Serilog.*`, `Aspire.Microsoft.EntityFrameworkCore.SqlServer`
- Persistencia: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
- Aplicación/DDD: `MediatR`, `ErrorOr`, `Ardalis.Specification.EntityFrameworkCore`
- Seguridad: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- Caché: `ZiggyCreatures.FusionCache`, `ZiggyCreatures.FusionCache.Serialization.SystemTextJson`
- IA: `Microsoft.Extensions.AI`, `GeminiDotnet.Extensions.AI`
- Aspire/Recursos: `Aspire.Hosting.AppHost`, `Aspire.Hosting.SqlServer`, `BCat.Aspire.MailDev`

## Requisitos previos

- .NET 10 SDK (preview)
- Visual Studio 2022 (última versión con soporte para .NET 10) o CLI de .NET
- Docker Desktop (recomendado para recursos Aspire como SQL Server y MailDev)

## Cómo ejecutar

Restaurar y compilar

- Visual Studio: Abrir la solución, restaurar paquetes y compilar
- CLI:
  - `dotnet restore`
  - `dotnet build -c Debug`

Ejecutar con Aspire

- Visual Studio
  - Establecer `MyModularMonolith.AppHost` como proyecto de inicio
  - Iniciar depuración (abrirá Aspire Dashboard y levantará API + SQL Server + MailDev)
- CLI
  - `dotnet run --project ./MyModularMonolith.AppHost/MyModularMonolith.AppHost.csproj`

## API y documentación

- Swagger/OpenAPI está habilitado en `MyModularMonolith.Api`. Al ejecutar, abre la URL que muestra el Aspire Dashboard
- El repositorio incluye archivos `*.http` (p. ej. `MyModularMonolith.Api/Gyms.http`) para probar rápidamente desde VS/VS Code

## Configuración

- User Secrets
  - `MyModularMonolith.Api` y `MyModularMonolith.AppHost` definen `UserSecretsId`
  - Guarda valores sensibles (JWT, claves de IA, etc.) con User Secrets
- SQL Server
  - Suministrado por Aspire (`Aspire.Hosting.SqlServer`)
  - La cadena de conexión se inyecta/gestiona en tiempo de ejecución por Aspire
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

- Añadir `FusionCache` L2 (distribuida) y políticas de caché por caso de uso
- Ampliar casos de IA y configuración de proveedores
- Endurecer observabilidad con pipeline OTel completo y dashboards
- Más ejemplos end-to-end (Users/Gyms, eventos de dominio, tests de integración)

---
Construido con `net10.0` y paquetes preview; sujeto a cambios en futuras versiones.