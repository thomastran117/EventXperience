namespace backend.Models;

public class Club
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Clubtype { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public double? Rating { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
}