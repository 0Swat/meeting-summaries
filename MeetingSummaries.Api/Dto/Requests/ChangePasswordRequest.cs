namespace MeetingSummaries.Api.Dto.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
