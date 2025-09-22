using System.Text.Json;

namespace MyModularMonolith.Modules.Gyms.Domain.ValueObjects;

public record WeeklySchedule
{
    private readonly Dictionary<DayOfWeek, List<TimeSlot>> _schedule = new();

    public WeeklySchedule()
    {
    }

    public WeeklySchedule(Dictionary<DayOfWeek, List<TimeSlot>> schedule)
    {
        _schedule = schedule ?? new Dictionary<DayOfWeek, List<TimeSlot>>();
    }

    public IReadOnlyDictionary<DayOfWeek, List<TimeSlot>> Schedule => _schedule.AsReadOnly();

    public List<TimeSlot> GetTimeSlotsForDay(DayOfWeek dayOfWeek)
    {
        return _schedule.GetValueOrDefault(dayOfWeek, new List<TimeSlot>());
    }

    public bool HasTimeSlotsForDay(DayOfWeek dayOfWeek)
    {
        return _schedule.ContainsKey(dayOfWeek) && _schedule[dayOfWeek].Count > 0;
    }

    public bool IsValidReservationTime(DateTime reservationDateTime)
    {
        var dayOfWeek = reservationDateTime.DayOfWeek;
        var timeSlots = GetTimeSlotsForDay(dayOfWeek);

        return timeSlots.Any(slot => slot.Contains(reservationDateTime.TimeOfDay));
    }

    public WeeklySchedule AddTimeSlot(DayOfWeek dayOfWeek, TimeSlot timeSlot)
    {
        var newSchedule = new Dictionary<DayOfWeek, List<TimeSlot>>(_schedule);

        if (!newSchedule.ContainsKey(dayOfWeek))
            newSchedule[dayOfWeek] = new List<TimeSlot>();

        newSchedule[dayOfWeek].Add(timeSlot);
        newSchedule[dayOfWeek].Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

        return new WeeklySchedule(newSchedule);
    }

    public WeeklySchedule RemoveTimeSlot(DayOfWeek dayOfWeek, TimeSlot timeSlot)
    {
        var newSchedule = new Dictionary<DayOfWeek, List<TimeSlot>>(_schedule);

        if (newSchedule.ContainsKey(dayOfWeek))
        {
            newSchedule[dayOfWeek].Remove(timeSlot);
            if (newSchedule[dayOfWeek].Count == 0)
                newSchedule.Remove(dayOfWeek);
        }

        return new WeeklySchedule(newSchedule);
    }

    // Static factory methods
    public static WeeklySchedule Empty() => new();

    public static WeeklySchedule FromJsonString(string? jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return Empty();

        try
        {
            // Normalizar JSON (convertir comillas simples a dobles)
            var normalizedJson = jsonString.Replace("'", "\"");

            // Deserializar el JSON con claves en minúsculas
            var scheduleDict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(normalizedJson);
            if (scheduleDict == null)
                return Empty();

            var schedule = new Dictionary<DayOfWeek, List<TimeSlot>>();

            foreach (var kvp in scheduleDict)
            {
                if (TryParseDayOfWeek(kvp.Key, out var dayOfWeek))
                {
                    var timeSlots = kvp.Value
                        .Select(TimeSlot.Parse)
                        .Where(ts => ts != null)
                        .Cast<TimeSlot>()
                        .OrderBy(ts => ts.StartTime)
                        .ToList();

                    if (timeSlots.Count > 0)
                        schedule[dayOfWeek] = timeSlots;
                }
            }

            return new WeeklySchedule(schedule);
        }
        catch
        {
            return Empty();
        }
    }

    public string ToJsonString()
    {
        if (_schedule.Count == 0)
            return "{}";

        var scheduleDict = new Dictionary<string, List<string>>();

        foreach (var kvp in _schedule)
        {
            var dayKey = GetDayKey(kvp.Key);
            var timeSlots = kvp.Value.Select(ts => ts.ToString()).ToList();
            scheduleDict[dayKey] = timeSlots;
        }

        return JsonSerializer.Serialize(scheduleDict);
    }

    private static bool TryParseDayOfWeek(string dayKey, out DayOfWeek dayOfWeek)
    {
        dayOfWeek = dayKey.ToLowerInvariant() switch
        {
            "monday" => DayOfWeek.Monday,
            "tuesday" => DayOfWeek.Tuesday,
            "wednesday" => DayOfWeek.Wednesday,
            "thursday" => DayOfWeek.Thursday,
            "friday" => DayOfWeek.Friday,
            "saturday" => DayOfWeek.Saturday,
            "sunday" => DayOfWeek.Sunday,
            _ => default
        };

        return dayKey.ToLowerInvariant() is "monday" or "tuesday" or "wednesday" or
               "thursday" or "friday" or "saturday" or "sunday";
    }

    private static string GetDayKey(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => "monday",
        DayOfWeek.Tuesday => "tuesday",
        DayOfWeek.Wednesday => "wednesday",
        DayOfWeek.Thursday => "thursday",
        DayOfWeek.Friday => "friday",
        DayOfWeek.Saturday => "saturday",
        DayOfWeek.Sunday => "sunday",
        _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
    };

    public override string ToString()
    {
        if (_schedule.Count == 0)
            return "No schedule defined";

        var scheduleText = _schedule
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value)}")
            .ToList();

        return string.Join(" | ", scheduleText);
    }
}
