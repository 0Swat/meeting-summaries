using MeetingSummaries.Api.Models;

namespace MeetingSummaries.Api.Dto.Responses;

public record MeetingSummaryDto(
    Guid Id,
    MeetingType Type,
    DateOnly Date,
    List<MeetingPointDto> Points
);
