using Microsoft.AspNetCore.Identity;

namespace MyModularMonolith.Modules.Users.Domain;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public ApplicationRole() : base()
    {
        Id = Guid.NewGuid();
    }

    public ApplicationRole(string roleName, string description, DateTime createdAt) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpper();
        Description = description;
        CreatedAt = createdAt;
    }
}