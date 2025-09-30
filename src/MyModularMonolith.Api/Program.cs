using MediatR;
using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.AI;
using MyModularMonolith.Modules.Gyms;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Infrastructure;
using MyModularMonolith.Modules.Users;
using MyModularMonolith.Modules.Users.Infrastructure;
using Serilog;
using System.Reflection;

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

builder.Services.AddUsersModule(builder.Configuration, mediatRAssemblies);
builder.Services.AddGymsModule(builder.Configuration, mediatRAssemblies);
builder.Services.AddAIModule(builder.Configuration);

builder.Services.AddMediatR(mediatRAssemblies.ToArray());

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

