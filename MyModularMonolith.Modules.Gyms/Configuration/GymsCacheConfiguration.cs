namespace MyModularMonolith.Modules.Gyms.Configuration;

public class GymsCacheConfiguration
{
    public int DefaultDurationMinutes { get; set; } = 10;
    public int DefaultSize { get; set; } = 1;
    public int JitterMaxSeconds { get; set; } = 5;
    public CacheDurations Durations { get; set; } = new();
}

public class CacheDurations
{
    public int Gym { get; set; } = 15;
    public int GymList { get; set; } = 5;
    public int GymProducts { get; set; } = 20;
    public int Product { get; set; } = 30;
    public int PopularGyms { get; set; } = 60;
}