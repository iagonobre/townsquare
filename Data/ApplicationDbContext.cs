using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Townsquare.Models;

namespace Townsquare.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Event> Events { get; set; }
    public DbSet<RSVP> Rsvps { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}