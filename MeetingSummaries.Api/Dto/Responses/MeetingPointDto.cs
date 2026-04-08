namespace MeetingSummaries.Api.Dto.Responses;

public record MeetingPointDto(
    Guid Id,
    string Content,
    int OrderIndex
);
