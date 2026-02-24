using MediatR;
using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Api.Extensions;
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

builder.Services.AddUsersModule(mediatRAssemblies);
builder.Services.AddGymsModule(builder.Configuration, mediatRAssemblies);
builder.Services.AddAIModule(builder.Configuration);

builder.Services.AddMediatR(mediatRAssemblies.ToArray());

// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Rate Limiting
builder.Services.AddApiRateLimiting();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enhanced request logging with Serilog
app.UseEnhancedRequestLogging();

// Rate Limiting (BEFORE Authentication)
app.UseRateLimiter();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map module endpoints
app.MapUsersEndpoints();
app.MapGymsEndpoints();
app.MapAIEndpoints();

// Development endpoints
if (app.Environment.IsDevelopment())
{
    app.MapDevelopmentEndpoints();
}

// Health check endpoint
app.MapGet("/health", () => "Healthy")
    .WithName("HealthCheck");

app.Run();

