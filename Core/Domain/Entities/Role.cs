using Microsoft.AspNetCore.Identity;

namespace Core.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    public ICollection<UserRole> Users { get; set; } = [];
}