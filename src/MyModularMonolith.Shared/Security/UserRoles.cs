namespace MyModularMonolith.Shared.Security;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string User = "User";

    public static readonly string[] AllRoles = [SuperAdmin, User];

    public static readonly Dictionary<string, string> RoleDescriptions = new()
{
{ SuperAdmin, "Super Administrator with full system access" },
{ User, "Client/Member of the gym chain" }
};

    public static bool IsValidRole(string role) => AllRoles.Contains(role);
}
