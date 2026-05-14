using System.Text;
using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.Dtos;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Generates basic iCal output for productivity calendar events.
/// </summary>
public sealed class CalendarExportService : ICalendarExportService
{
    public Task<string> ExportICalendarAsync(IReadOnlyCollection<CalendarEventDto> events, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BEGIN:VCALENDAR");
        builder.AppendLine("VERSION:2.0");
        builder.AppendLine("PRODID:-//Alphabet//Productivity//EN");

        foreach (var calendarEvent in events)
        {
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:{calendarEvent.Id}");
            builder.AppendLine($"DTSTART:{calendarEvent.StartTime.UtcDateTime:yyyyMMddTHHmmssZ}");
            builder.AppendLine($"DTEND:{calendarEvent.EndTime.UtcDateTime:yyyyMMddTHHmmssZ}");
            builder.AppendLine($"SUMMARY:{Escape(calendarEvent.Title)}");
            builder.AppendLine($"DESCRIPTION:{Escape(calendarEvent.Description)}");
            if (!string.IsNullOrWhiteSpace(calendarEvent.Location))
            {
                builder.AppendLine($"LOCATION:{Escape(calendarEvent.Location)}");
            }

            builder.AppendLine("END:VEVENT");
        }

        builder.AppendLine("END:VCALENDAR");
        return Task.FromResult(builder.ToString());
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace(",", "\\,").Replace(";", "\\;").Replace("\r", string.Empty).Replace("\n", "\\n");
}
