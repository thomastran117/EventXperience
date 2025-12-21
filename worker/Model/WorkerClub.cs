using worker.Common;

namespace worker.Models
{
    public class Club
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required ClubType Clubtype { get; set; }
        public required string ClubImage { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public double? Rating { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Location { get; set; }
        public bool IsVerified { get; set; } = false;
        public int MemberCount { get; set; } = 0;
        public int UserId { get; set; }
    }
}
