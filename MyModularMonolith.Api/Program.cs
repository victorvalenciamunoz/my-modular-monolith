using Microsoft.EntityFrameworkCore;
using MyModularMonolith.Modules.AI;
using MyModularMonolith.Modules.Gyms;
using MyModularMonolith.Modules.Gyms.Infrastructure;
using MyModularMonolith.Modules.Users;
using MyModularMonolith.Modules.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add modules
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddGymsModule(builder.Configuration);
builder.Services.AddAIModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();

    var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await usersContext.Database.MigrateAsync();

    var gymsContext = scope.ServiceProvider.GetRequiredService<GymsDbContext>();
    await gymsContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();

// Authentication & Authorization middleware (ORDER IS IMPORTANT)
app.UseAuthentication();
app.UseAuthorization();

// Map module endpoints
app.MapUsersEndpoints();
app.MapGymsEndpoints();
app.MapAIEndpoints();

// Health check endpoint
app.MapGet("/health", () => "Healthy")
    .WithName("HealthCheck");    

app.Run();

