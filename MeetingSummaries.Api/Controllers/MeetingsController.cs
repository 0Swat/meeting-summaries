using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingSummaries.Api.Dto.Requests;
using MeetingSummaries.Api.Dto.Responses;
using MeetingSummaries.Api.Models;
using MeetingSummaries.Api.Services;

namespace MeetingSummaries.Api.Controllers;

/// <summary>
/// Zarządzanie podsumowaniami spotkań i ich punktami.
/// </summary>
[Authorize]
[ApiController]
[Route("api/meetings")]
[Produces("application/json")]
public class MeetingsController(MeetingService service) : ControllerBase
{
    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Zwraca listę wszystkich dostępnych typów spotkań.</summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetTypes() => Ok(service.GetMeetingTypes());

    /// <summary>Zwraca dane do wyświetlenia kropek na kalendarzu dla danego miesiąca.</summary>
    [HttpGet("month/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(List<DayDotsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthDots(int year, int month) =>
        Ok(await service.GetMonthDotsAsync(year, month, UserId));

    /// <summary>Zwraca wszystkie podsumowania spotkań z danego dnia.</summary>
    [HttpGet("by-date/{date}")]
    [ProducesResponseType(typeof(List<MeetingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate(DateOnly date) =>
        Ok(await service.GetSummariesForDateAsync(date, UserId));

    /// <summary>Zwraca listę dat z istniejącymi podsumowaniami dla danego typu.</summary>
    [HttpGet("{type}/dates")]
    [ProducesResponseType(typeof(List<DateOnly>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDates(MeetingType type) =>
        Ok(await service.GetDatesForTypeAsync(type, UserId));

    /// <summary>Zwraca podsumowanie konkretnego spotkania wraz z punktami.</summary>
    [HttpGet("{type}/{date}")]
    [ProducesResponseType(typeof(MeetingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(MeetingType type, DateOnly date)
    {
        var result = await service.GetSummaryAsync(type, date, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Tworzy podsumowanie spotkania jeśli nie istnieje (idempotentne).</summary>
    [HttpPost("{type}/{date}")]
    [ProducesResponseType(typeof(MeetingSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnsureSummary(MeetingType type, DateOnly date) =>
        Ok(await service.EnsureSummaryAsync(type, date, UserId));

    /// <summary>Usuwa podsumowanie spotkania wraz ze wszystkimi punktami.</summary>
    [HttpDelete("{type}/{date}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSummary(MeetingType type, DateOnly date)
    {
        try
        {
            await service.DeleteSummaryAsync(type, date, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Dodaje nowy punkt do podsumowania.</summary>
    [HttpPost("{type}/{date}/points")]
    [ProducesResponseType(typeof(MeetingPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPoint(
        MeetingType type, DateOnly date, [FromBody] AddPointRequest request)
    {
        try
        {
            return Ok(await service.AddPointAsync(type, date, UserId, request));
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Aktualizuje treść istniejącego punktu.</summary>
    [HttpPut("points/{pointId:guid}")]
    [ProducesResponseType(typeof(MeetingPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePoint(Guid pointId, [FromBody] UpdatePointRequest request)
    {
        try
        {
            return Ok(await service.UpdatePointAsync(pointId, UserId, request));
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Usuwa punkt spotkania.</summary>
    [HttpDelete("points/{pointId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePoint(Guid pointId)
    {
        try
        {
            await service.DeletePointAsync(pointId, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Zmienia kolejność punktów w ramach podsumowania.</summary>
    [HttpPatch("points/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderPoints([FromBody] ReorderPointsRequest request)
    {
        try
        {
            await service.ReorderPointsAsync(request, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }
}
