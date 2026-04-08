using Microsoft.EntityFrameworkCore;
using MeetingSummaries.Api.Data;
using MeetingSummaries.Api.Dto.Requests;
using MeetingSummaries.Api.Dto.Responses;
using MeetingSummaries.Api.Models;

namespace MeetingSummaries.Api.Services;

public class MeetingService(AppDbContext db)
{
    public IEnumerable<string> GetMeetingTypes() =>
        Enum.GetNames<MeetingType>();

    public async Task<List<DateOnly>> GetDatesForTypeAsync(MeetingType type) =>
        await db.MeetingSummaries
            .Where(s => s.Type == type)
            .OrderByDescending(s => s.Date)
            .Select(s => s.Date)
            .ToListAsync();

    public async Task<List<DayDotsDto>> GetMonthDotsAsync(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

        var rows = await db.MeetingSummaries
            .Where(s => s.Date >= from && s.Date < to)
            .Select(s => new { s.Date, s.Type })
            .ToListAsync();

        return rows
            .GroupBy(r => r.Date)
            .Select(g => new DayDotsDto(g.Key, g.Select(r => r.Type).OrderBy(t => t).ToList()))
            .OrderBy(d => d.Date)
            .ToList();
    }

    public async Task<List<MeetingSummaryDto>> GetSummariesForDateAsync(DateOnly date)
    {
        var summaries = await db.MeetingSummaries
            .Include(s => s.Points.OrderBy(p => p.OrderIndex))
            .Where(s => s.Date == date)
            .ToListAsync();

        return summaries
            .OrderBy(s => s.Type)
            .Select(ToDto)
            .ToList();
    }

    public async Task<MeetingSummaryDto?> GetSummaryAsync(MeetingType type, DateOnly date)
    {
        var summary = await db.MeetingSummaries
            .Include(s => s.Points.OrderBy(p => p.OrderIndex))
            .FirstOrDefaultAsync(s => s.Type == type && s.Date == date);

        return summary is null ? null : ToDto(summary);
    }

    public async Task<MeetingSummaryDto> EnsureSummaryAsync(MeetingType type, DateOnly date)
    {
        var existing = await db.MeetingSummaries
            .Include(s => s.Points.OrderBy(p => p.OrderIndex))
            .FirstOrDefaultAsync(s => s.Type == type && s.Date == date);

        if (existing is not null)
            return ToDto(existing);

        var summary = new MeetingSummary
        {
            Id = Guid.NewGuid(),
            Type = type,
            Date = date
        };
        db.MeetingSummaries.Add(summary);
        await db.SaveChangesAsync();
        return ToDto(summary);
    }

    public async Task<MeetingPointDto> AddPointAsync(MeetingType type, DateOnly date, AddPointRequest request)
    {
        var summary = await db.MeetingSummaries
            .Include(s => s.Points)
            .FirstOrDefaultAsync(s => s.Type == type && s.Date == date)
            ?? throw new KeyNotFoundException("Summary not found.");

        var maxIndex = summary.Points.Any()
            ? summary.Points.Max(p => p.OrderIndex)
            : -1;

        var point = new MeetingPoint
        {
            Id = Guid.NewGuid(),
            SummaryId = summary.Id,
            Content = request.Content,
            OrderIndex = maxIndex + 1
        };
        db.MeetingPoints.Add(point);
        await db.SaveChangesAsync();
        return new MeetingPointDto(point.Id, point.Content, point.OrderIndex);
    }

    public async Task<MeetingPointDto> UpdatePointAsync(Guid pointId, UpdatePointRequest request)
    {
        var point = await db.MeetingPoints.FindAsync(pointId)
            ?? throw new KeyNotFoundException("Point not found.");

        point.Content = request.Content;
        await db.SaveChangesAsync();
        return new MeetingPointDto(point.Id, point.Content, point.OrderIndex);
    }

    public async Task DeletePointAsync(Guid pointId)
    {
        var point = await db.MeetingPoints.FindAsync(pointId)
            ?? throw new KeyNotFoundException("Point not found.");

        db.MeetingPoints.Remove(point);
        await db.SaveChangesAsync();
    }

    public async Task DeleteSummaryAsync(MeetingType type, DateOnly date)
    {
        var summary = await db.MeetingSummaries
            .FirstOrDefaultAsync(s => s.Type == type && s.Date == date)
            ?? throw new KeyNotFoundException("Summary not found.");

        db.MeetingSummaries.Remove(summary);
        await db.SaveChangesAsync();
    }

    public async Task ReorderPointsAsync(ReorderPointsRequest request)
    {
        var ids = request.Items.Select(i => i.PointId).ToList();
        var points = await db.MeetingPoints
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();

        foreach (var item in request.Items)
        {
            var point = points.FirstOrDefault(p => p.Id == item.PointId)
                ?? throw new KeyNotFoundException($"Point {item.PointId} not found.");
            point.OrderIndex = item.NewIndex;
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static MeetingSummaryDto ToDto(MeetingSummary s) =>
        new(
            s.Id,
            s.Type,
            s.Date,
            s.Points
                .OrderBy(p => p.OrderIndex)
                .Select(p => new MeetingPointDto(p.Id, p.Content, p.OrderIndex))
                .ToList()
        );
}
