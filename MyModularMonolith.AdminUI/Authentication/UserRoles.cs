namespace MyModularMonolith.AdminUI.Authentication;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";

    public static readonly string[] AllRoles = [SuperAdmin];

    public static readonly Dictionary<string, string> RoleDescriptions = new()
{
{ SuperAdmin, "Super Administrator with full system access" }
};

    public static bool IsValidRole(string role) => AllRoles.Contains(role);
}
