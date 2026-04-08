using Microsoft.AspNetCore.Mvc;
using MeetingSummaries.Api.Dto.Requests;
using MeetingSummaries.Api.Dto.Responses;
using MeetingSummaries.Api.Models;
using MeetingSummaries.Api.Services;

namespace MeetingSummaries.Api.Controllers;

/// <summary>
/// Zarządzanie podsumowaniami spotkań i ich punktami.
/// </summary>
[ApiController]
[Route("api/meetings")]
[Produces("application/json")]
public class MeetingsController(MeetingService service) : ControllerBase
{
    /// <summary>
    /// Zwraca listę wszystkich dostępnych typów spotkań.
    /// </summary>
    /// <returns>Tablica nazw typów: Daily, Refinement, Retro, SprintReview, SprintPlanning.</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetTypes() => Ok(service.GetMeetingTypes());

    /// <summary>
    /// Zwraca dane do wyświetlenia kropek na kalendarzu dla danego miesiąca.
    /// </summary>
    /// <param name="year">Rok (np. 2026).</param>
    /// <param name="month">Miesiąc 1–12.</param>
    /// <returns>Lista dni z przypisanymi typami spotkań.</returns>
    [HttpGet("month/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(List<DayDotsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthDots(int year, int month) =>
        Ok(await service.GetMonthDotsAsync(year, month));

    /// <summary>
    /// Zwraca wszystkie podsumowania spotkań z danego dnia (wszystkie typy).
    /// </summary>
    /// <param name="date">Data w formacie yyyy-MM-dd.</param>
    /// <returns>Lista podsumowań wraz z punktami posortowanymi po OrderIndex.</returns>
    [HttpGet("by-date/{date}")]
    [ProducesResponseType(typeof(List<MeetingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate(DateOnly date) =>
        Ok(await service.GetSummariesForDateAsync(date));

    /// <summary>
    /// Zwraca listę dat, dla których istnieje podsumowanie danego typu spotkania.
    /// </summary>
    /// <param name="type">Typ spotkania.</param>
    /// <returns>Daty posortowane malejąco.</returns>
    [HttpGet("{type}/dates")]
    [ProducesResponseType(typeof(List<DateOnly>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDates(MeetingType type) =>
        Ok(await service.GetDatesForTypeAsync(type));

    /// <summary>
    /// Zwraca podsumowanie konkretnego spotkania wraz z punktami.
    /// </summary>
    /// <param name="type">Typ spotkania.</param>
    /// <param name="date">Data w formacie yyyy-MM-dd.</param>
    [HttpGet("{type}/{date}")]
    [ProducesResponseType(typeof(MeetingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(MeetingType type, DateOnly date)
    {
        var result = await service.GetSummaryAsync(type, date);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Tworzy podsumowanie spotkania, jeśli jeszcze nie istnieje (idempotentne).
    /// </summary>
    /// <param name="type">Typ spotkania.</param>
    /// <param name="date">Data w formacie yyyy-MM-dd.</param>
    /// <returns>Istniejące lub nowo utworzone podsumowanie.</returns>
    [HttpPost("{type}/{date}")]
    [ProducesResponseType(typeof(MeetingSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnsureSummary(MeetingType type, DateOnly date)
    {
        var result = await service.EnsureSummaryAsync(type, date);
        return Ok(result);
    }

    /// <summary>
    /// Usuwa podsumowanie spotkania wraz ze wszystkimi jego punktami.
    /// </summary>
    /// <param name="type">Typ spotkania.</param>
    /// <param name="date">Data w formacie yyyy-MM-dd.</param>
    [HttpDelete("{type}/{date}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Dodaje nowy punkt do podsumowania. OrderIndex jest automatycznie ustawiany jako MAX + 1.
    /// </summary>
    /// <param name="type">Typ spotkania.</param>
    /// <param name="date">Data w formacie yyyy-MM-dd.</param>
    /// <param name="request">Treść nowego punktu.</param>
    [HttpPost("{type}/{date}/points")]
    [ProducesResponseType(typeof(MeetingPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Aktualizuje treść istniejącego punktu.
    /// </summary>
    /// <param name="pointId">ID punktu.</param>
    /// <param name="request">Nowa treść punktu.</param>
    [HttpPut("points/{pointId:guid}")]
    [ProducesResponseType(typeof(MeetingPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Usuwa punkt spotkania.
    /// </summary>
    /// <param name="pointId">ID punktu.</param>
    [HttpDelete("points/{pointId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Zmienia kolejność punktów w ramach podsumowania. Operacja wykonywana w transakcji.
    /// </summary>
    /// <param name="request">Lista par (pointId, newIndex) dla wszystkich przestawianych punktów.</param>
    [HttpPatch("points/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
