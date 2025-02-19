using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TestIdentityCore.Domain;

public class ApplicationUser: IdentityUser<Guid>
{
    [MaxLength(100)]
    public string? FullName { get; set; }
}