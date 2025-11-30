using System.ComponentModel.DataAnnotations;

namespace Townsquare.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public string? CreatorId { get; set; }
    public ApplicationUser? Creator { get; set; }


    public bool IsOrphaned { get; set; } = false;

    public ICollection<RSVP> Rsvps { get; set; } = new List<RSVP>();
}