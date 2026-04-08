using System.Text.Json.Serialization;

namespace MeetingSummaries.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeetingType
{
    Daily,
    Refinement,
    Retro,
    SprintReview,
    SprintPlanning
}

public class MeetingSummary
{
    public Guid Id { get; set; }
    public MeetingType Type { get; set; }
    public DateOnly Date { get; set; }
    public ICollection<MeetingPoint> Points { get; set; } = new List<MeetingPoint>();
}
