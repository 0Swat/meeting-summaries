namespace MeetingSummaries.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<MeetingSummary> Summaries { get; set; } = new List<MeetingSummary>();
}
