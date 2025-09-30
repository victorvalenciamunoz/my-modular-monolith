namespace MyModularMonolith.Modules.Gyms.Domain.ValueObjects;

public record TimeSlot(TimeSpan StartTime, TimeSpan EndTime) : IComparable<TimeSlot>
{
    public static TimeSlot? Parse(string timeSlotString)
    {
        if (string.IsNullOrWhiteSpace(timeSlotString))
            return null;

        try
        {
            var parts = timeSlotString.Split('-');
            if (parts.Length != 2) return null;

            if (!TimeSpan.TryParse(parts[0].Trim(), out var startTime) ||
                !TimeSpan.TryParse(parts[1].Trim(), out var endTime))
                return null;

            if (startTime >= endTime)
                return null;

            return new TimeSlot(startTime, endTime);
        }
        catch
        {
            return null;
        }
    }

    public bool Contains(TimeSpan time) => time >= StartTime && time < EndTime;

    public bool Contains(DateTime dateTime) => Contains(dateTime.TimeOfDay);

    public bool Overlaps(TimeSlot other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public TimeSpan Duration => EndTime - StartTime;

    public int CompareTo(TimeSlot? other)
    {
        if (other == null) return 1;
        return StartTime.CompareTo(other.StartTime);
    }

    public override string ToString() => $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
}
