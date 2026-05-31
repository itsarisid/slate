using Alphabet.Application.Features.Productivity.Dtos;

namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>
/// Exports and imports calendar data in iCal-compatible formats.
/// </summary>
public interface ICalendarExportService
{
    /// <summary>
    /// Export icalendar async.
    /// </summary>
    Task<string> ExportICalendarAsync(IReadOnlyCollection<CalendarEventDto> events, CancellationToken cancellationToken);
}
