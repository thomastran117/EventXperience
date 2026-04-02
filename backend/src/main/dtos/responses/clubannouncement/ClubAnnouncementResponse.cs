namespace backend.main.dtos.responses.clubannouncement
{
    public class ClubAnnouncementResponse
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ClubAnnouncementResponse(int id, int clubId, int userId, string title, string content, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            ClubId = clubId;
            UserId = userId;
            Title = title;
            Content = content;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
