using backend.main.models.enums;

namespace backend.main.dtos.responses.clubpost
{
    public class ClubPostResponse
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public PostType PostType { get; set; }
        public int LikesCount { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ClubPostResponse(int id, int clubId, int userId, string title, string content,
            PostType postType, int likesCount, bool isPinned, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            ClubId = clubId;
            UserId = userId;
            Title = title;
            Content = content;
            PostType = postType;
            LikesCount = likesCount;
            IsPinned = isPinned;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
