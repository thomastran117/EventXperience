using worker.Common;
using worker.DTOs;
using worker.Models;

namespace backend.Mappers
{
    public static class ClubCacheMapper
    {
        public static ClubCacheDto ToDto(Club club) => new(
            club.Id,
            club.Name,
            club.Description,
            club.Clubtype.ToString(),
            club.ClubImage,
            club.Phone,
            club.Email,
            club.MemberCount,
            club.IsVerified,
            club.UserId
        );

        public static Club ToEntity(ClubCacheDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Clubtype = Enum.Parse<ClubType>(dto.Clubtype, true),
            ClubImage = dto.ClubImage,
            Phone = dto.Phone,
            Email = dto.Email,
            MemberCount = dto.MemberCount,
            IsVerified = dto.IsVerified,
            UserId = dto.UserId,
            User = null!
        };
    }
}
