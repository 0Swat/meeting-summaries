using Microsoft.AspNetCore.Mvc;
using MeetingSummaries.Api.Dto.Requests;
using MeetingSummaries.Api.Models;
using MeetingSummaries.Api.Services;

namespace MeetingSummaries.Api.Controllers;

[ApiController]
[Route("api/meetings")]
public class MeetingsController(MeetingService service) : ControllerBase
{
    [HttpGet("types")]
    public IActionResult GetTypes() => Ok(service.GetMeetingTypes());

    [HttpGet("month/{year:int}/{month:int}")]
    public async Task<IActionResult> GetMonthDots(int year, int month) =>
        Ok(await service.GetMonthDotsAsync(year, month));

    [HttpGet("by-date/{date}")]
    public async Task<IActionResult> GetByDate(DateOnly date) =>
        Ok(await service.GetSummariesForDateAsync(date));

    [HttpGet("{type}/dates")]
    public async Task<IActionResult> GetDates(MeetingType type) =>
        Ok(await service.GetDatesForTypeAsync(type));

    [HttpGet("{type}/{date}")]
    public async Task<IActionResult> GetSummary(MeetingType type, DateOnly date)
    {
        var result = await service.GetSummaryAsync(type, date);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{type}/{date}")]
    public async Task<IActionResult> EnsureSummary(MeetingType type, DateOnly date)
    {
        var result = await service.EnsureSummaryAsync(type, date);
        return Ok(result);
    }

    [HttpDelete("{type}/{date}")]
    public async Task<IActionResult> DeleteSummary(MeetingType type, DateOnly date)
    {
        try
        {
            await service.DeleteSummaryAsync(type, date);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{type}/{date}/points")]
    public async Task<IActionResult> AddPoint(
        MeetingType type, DateOnly date, [FromBody] AddPointRequest request)
    {
        try
        {
            var point = await service.AddPointAsync(type, date, request);
            return Ok(point);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("points/{pointId:guid}")]
    public async Task<IActionResult> UpdatePoint(Guid pointId, [FromBody] UpdatePointRequest request)
    {
        try
        {
            var point = await service.UpdatePointAsync(pointId, request);
            return Ok(point);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("points/{pointId:guid}")]
    public async Task<IActionResult> DeletePoint(Guid pointId)
    {
        try
        {
            await service.DeletePointAsync(pointId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("points/reorder")]
    public async Task<IActionResult> ReorderPoints([FromBody] ReorderPointsRequest request)
    {
        try
        {
            await service.ReorderPointsAsync(request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
