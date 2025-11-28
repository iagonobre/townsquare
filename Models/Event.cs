namespace Townsquare.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
    public DateTime Date { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Address { get; set; }
    
    public string CreatorId { get; set; }
    public ApplicationUser Creator { get; set; }
    public bool IsOrphaned { get; set; } = false;
}