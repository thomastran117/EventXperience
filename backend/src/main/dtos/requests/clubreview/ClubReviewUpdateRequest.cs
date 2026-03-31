using System.ComponentModel.DataAnnotations;

namespace backend.main.dtos.requests.clubreview
{
    public class ClubReviewUpdateRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }
    }
}
