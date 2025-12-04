namespace backend.Models;

public class EventClub
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public required string EventImage { get; set; }
    public string? Intensity { get; set; }
    public required DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int ClubId { get; set; }
    public required Club Club { get; set; }
}
