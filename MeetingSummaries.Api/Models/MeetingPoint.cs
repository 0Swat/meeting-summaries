namespace MeetingSummaries.Api.Models;

public class MeetingPoint
{
    public Guid Id { get; set; }
    public Guid SummaryId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public MeetingSummary Summary { get; set; } = null!;
}
