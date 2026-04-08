using MeetingSummaries.Api.Models;

namespace MeetingSummaries.Api.Dto.Responses;

public record DayDotsDto(DateOnly Date, List<MeetingType> Types);
