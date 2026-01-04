using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for working with dates, times, and schedules
/// </summary>
public class TimeTool : ITool
{
    public string Name => "time";
    public string Description => "Get current time, calculate time differences, and work with dates";

    public Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var action = parameters.GetValueOrDefault("action")?.ToString();

        var result = action switch
        {
            "current_time" => GetCurrentTime(),
            "time_difference" => CalculateTimeDifference(parameters),
            "add_time" => AddTime(parameters),
            "format_date" => FormatDate(parameters),
            "is_business_hours" => IsBusinessHours(parameters),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };

        return Task.FromResult<object>(result);
    }

    private object GetCurrentTime()
    {
        var now = DateTime.UtcNow;
        return new
        {
            utc = now,
            unix_timestamp = new DateTimeOffset(now).ToUnixTimeSeconds(),
            iso8601 = now.ToString("O")
        };
    }

    private object CalculateTimeDifference(Dictionary<string, object> parameters)
    {
        var start = DateTime.Parse(parameters.GetValueOrDefault("start")?.ToString() ?? throw new ArgumentException("start required"));
        var end = DateTime.Parse(parameters.GetValueOrDefault("end")?.ToString() ?? throw new ArgumentException("end required"));

        var diff = end - start;

        return new
        {
            start,
            end,
            totalDays = diff.TotalDays,
            totalHours = diff.TotalHours,
            totalMinutes = diff.TotalMinutes,
            formatted = $"{diff.Days}d {diff.Hours}h {diff.Minutes}m"
        };
    }

    private object AddTime(Dictionary<string, object> parameters)
    {
        var dateTime = DateTime.Parse(parameters.GetValueOrDefault("datetime")?.ToString() ?? DateTime.UtcNow.ToString());
        var days = Convert.ToInt32(parameters.GetValueOrDefault("days") ?? 0);
        var hours = Convert.ToInt32(parameters.GetValueOrDefault("hours") ?? 0);
        var minutes = Convert.ToInt32(parameters.GetValueOrDefault("minutes") ?? 0);

        var result = dateTime.AddDays(days).AddHours(hours).AddMinutes(minutes);

        return new
        {
            original = dateTime,
            added = new { days, hours, minutes },
            result
        };
    }

    private object FormatDate(Dictionary<string, object> parameters)
    {
        var dateTime = DateTime.Parse(parameters.GetValueOrDefault("datetime")?.ToString() ?? DateTime.UtcNow.ToString());
        var format = parameters.GetValueOrDefault("format")?.ToString() ?? "O";

        return new
        {
            datetime = dateTime,
            format,
            formatted = dateTime.ToString(format)
        };
    }

    private object IsBusinessHours(Dictionary<string, object> parameters)
    {
        var dateTime = DateTime.Parse(parameters.GetValueOrDefault("datetime")?.ToString() ?? DateTime.UtcNow.ToString());
        var startHour = Convert.ToInt32(parameters.GetValueOrDefault("start_hour") ?? 9);
        var endHour = Convert.ToInt32(parameters.GetValueOrDefault("end_hour") ?? 17);

        var isWeekday = dateTime.DayOfWeek >= DayOfWeek.Monday && dateTime.DayOfWeek <= DayOfWeek.Friday;
        var isWithinHours = dateTime.Hour >= startHour && dateTime.Hour < endHour;

        return new
        {
            datetime = dateTime,
            isWeekday,
            isWithinHours,
            isBusinessHours = isWeekday && isWithinHours
        };
    }
}
