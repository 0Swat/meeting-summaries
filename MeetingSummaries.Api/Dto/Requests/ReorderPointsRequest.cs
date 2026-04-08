namespace MeetingSummaries.Api.Dto.Requests;

public record ReorderItem(Guid PointId, int NewIndex);

public record ReorderPointsRequest(List<ReorderItem> Items);
