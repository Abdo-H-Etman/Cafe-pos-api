using Microsoft.AspNetCore.Identity;

namespace Core.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; } = null!;
    public Guid BranchId { get; set; }
    public DateTime DateJoined { get; set; }

    public Branch Branch { get; set; } = null!;
}