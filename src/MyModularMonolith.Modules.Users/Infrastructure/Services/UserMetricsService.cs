using MyModularMonolith.Modules.Users.Application.Services;
using System.Diagnostics.Metrics;

namespace MyModularMonolith.Modules.Users.Infrastructure.Services;

internal class UserMetricsService : IUserMetricsService
{
    private readonly Counter<int> _userRegistrationCounter;

    public UserMetricsService()
    {
        var meter = new Meter("MyModularMonolith.Users", "1.0.0");
        _userRegistrationCounter = meter.CreateCounter<int>("UserRegistrations", 
            description: "Total number of user registrations");
    }

    public void IncrementUserRegistrations()
    {
        _userRegistrationCounter.Add(1);
    }
}