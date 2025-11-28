namespace Townsquare.Models;

public class RSVP
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}