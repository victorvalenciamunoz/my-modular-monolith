namespace MyModularMonolith.Modules.Gyms.Application.Cache;

internal static class GymsCacheKeys
{
    /// <summary>
    /// Cache key for the list of active gyms
    /// </summary>
    public const string ActiveGymsList = "gyms:list:active";

    /// <summary>
    /// Cache key pattern for individual gym by ID
    /// </summary>
    private const string GymByIdPattern = "gym:{0}";

    /// <summary>
    /// Gets the cache key for a specific gym by ID
    /// </summary>
    /// <param name="gymId">The gym identifier</param>
    /// <returns>The cache key for the gym</returns>
    public static string GetGymById(Guid gymId) => string.Format(GymByIdPattern, gymId);
}
