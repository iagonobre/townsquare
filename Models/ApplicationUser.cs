using Microsoft.AspNetCore.Identity;
namespace Townsquare.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public bool IsDeleted { get; set; } = false;
}