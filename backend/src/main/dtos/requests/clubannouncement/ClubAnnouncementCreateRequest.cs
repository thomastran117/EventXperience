using System.ComponentModel.DataAnnotations;

namespace backend.main.dtos.requests.clubannouncement
{
    public class ClubAnnouncementCreateRequest
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
