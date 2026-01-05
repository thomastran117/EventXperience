using backend.DTOs;
using backend.Models;

namespace backend.Mappers
{
    public static class EventMapper
    {
        public static EventResponse MapToResponse(Events ev) => new()
        {
            Id = ev.Id,
            Name = ev.Name,
            Description = ev.Description,
            Location = ev.Location,
            ImageUrl = ev.ImageUrl,
            IsPrivate = ev.isPrivate,
            MaxParticipants = ev.maxParticipants,
            RegisterCost = ev.registerCost,
            StartTime = ev.StartTime,
            EndTime = ev.EndTime,
            ClubId = ev.ClubId,
            CreatedAt = ev.CreatedAt
        };
    }
}
