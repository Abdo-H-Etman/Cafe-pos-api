using Microsoft.AspNetCore.Identity;

namespace Core.Domain.Entities;

public class UserRole : IdentityUserRole<Guid>
{
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}