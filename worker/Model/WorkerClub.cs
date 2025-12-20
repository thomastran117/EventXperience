using worker.Common;

namespace worker.Models
{
    public class Club
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ClubType Clubtype { get; set; }

        public bool IsVerified { get; set; }
        public int MemberCount { get; set; }
        public double? Rating { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
