using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyModularMonolith.Modules.AI;
using MyModularMonolith.Modules.Gyms;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Infrastructure;
using MyModularMonolith.Modules.Users;
using MyModularMonolith.Modules.Users.Infrastructure;
using Serilog;
using System.Reflection;
using System.Text;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
    .AddUserSecrets<Program>()
    .Build();


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Destructure.ByTransforming<GymProductDto>(x => new
        {
            x.GymId,
            x.GymName,
            x.ProductId,
            x.ProductName,
            x.CreatedAt,
            x.UpdatedAt
        }));

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<UsersDbContext>("MyModularMonolith",
    configureDbContextOptions: options =>
    {
        options.UseSqlServer(sqlOptions =>
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Users"));
    });

builder.AddSqlServerDbContext<GymsDbContext>("MyModularMonolith",
    configureDbContextOptions: options =>
    {
        options.UseSqlServer(sqlOptions =>
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Gyms"));
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

List<Assembly> mediatRAssemblies = [typeof(Program).Assembly];

builder.Services.AddUsersModule(mediatRAssemblies);
builder.Services.AddGymsModule(builder.Configuration, mediatRAssemblies);
builder.Services.AddAIModule(builder.Configuration);

builder.Services.AddMediatR(mediatRAssemblies.ToArray());

// Log JWT configuration being used
var jwtSecret = builder.Configuration["JWT:Secret"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

Log.Information("🔐 JWT Configuration: Issuer={Issuer}, Audience={Audience}, SecretLength={SecretLength}",
    jwtIssuer, jwtAudience, jwtSecret?.Length ?? 0);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Error("Authentication failed: {Error}", context.Exception.Message);
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        Log.Error("Token has expired");
                    }
                    else if (context.Exception is SecurityTokenInvalidSignatureException)
                    {
                        Log.Error("Token signature is invalid - Secret mismatch!");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Information("✅ Token validated successfully for user: {User}",
                        context.Principal?.Identity?.Name ?? "Unknown");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Log.Warning("⚠️ Authentication challenge: Error={Error}, ErrorDescription={ErrorDescription}",
                        context.Error ?? "null", context.ErrorDescription ?? "null");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        Log.Debug("📩 Authorization header received: {HeaderStart}...",
                            authHeader.Length > 50 ? authHeader.Substring(0, 50) : authHeader);
                    }
                    else
                    {
                        Log.Warning("⚠️ No Authorization header found in request");
                    }
                    return Task.CompletedTask;
                }
            };
        });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? Serilog.Events.LogEventLevel.Error
        : elapsed > 1000
            ? Serilog.Events.LogEventLevel.Warning
            : Serilog.Events.LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
    };
});

// Authentication & Authorization middleware (ORDER IS IMPORTANT)
app.UseAuthentication();
app.UseAuthorization();

// Map module endpoints
app.MapUsersEndpoints();
app.MapGymsEndpoints();
app.MapAIEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/migrate", async (IServiceProvider services) =>
    {
        using var scope = services.CreateScope();

        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        Log.Information("Starting Users module database migrations");
        await usersContext.Database.MigrateAsync();
        Log.Information("Users module database migrations applied");

        var gymsContext = scope.ServiceProvider.GetRequiredService<GymsDbContext>();
        Log.Information("Starting Gyms module database migrations");
        await gymsContext.Database.MigrateAsync();
        Log.Information("Gyms module database migrations applied");

        Log.Information("Database migrations completed successfully");
        return Results.Ok("Migrations completed successfully");
    })
    .WithName("RunMigrations")
    .WithTags("Database");
}

// Health check endpoint
app.MapGet("/health", () => "Healthy")
    .WithName("HealthCheck");

app.Run();

