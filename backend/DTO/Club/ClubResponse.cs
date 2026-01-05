using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class ClubResponse
    {
        public ClubResponse(int id, string name, string description, string clubtype, string clubimage)
        {
            Id = id;
            Name = name;
            Description = description;
            Clubtype = clubtype;
            ClubImage = clubimage;
        }

        [Required]
        public int Id
        {
            get; set;
        }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Clubtype { get; set; } = string.Empty;

        [Required]
        public string ClubImage { get; set; } = string.Empty;

        public string? Phone
        {
            get; set;
        }
        public string? Email
        {
            get; set;
        }
        public double? Rating
        {
            get; set;
        }
    }
}
