namespace MeetingSummaries.Api.Dto.Responses;

public record LoginResponse(string Token, DateTime ExpiresAt);
