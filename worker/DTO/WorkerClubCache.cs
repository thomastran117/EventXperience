namespace worker.DTOs
{
    public record ClubCacheDto(
        int Id,
        string Name,
        string Description,
        string Clubtype,
        string ClubImage,
        string? Phone,
        string? Email,
        int MemberCount,
        bool IsVerified,
        int UserId
    );
}
