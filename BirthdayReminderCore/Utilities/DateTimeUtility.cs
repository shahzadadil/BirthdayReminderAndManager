using System;

namespace BirthdayReminderCore.Utilities
{
    public static class DateTimeUtility
    {
        public static int? GetTimeToEvent(DateTime? eventDateTime)
        {
            if (!eventDateTime.HasValue)
            {
                return null;
            }

            var localEventTime = new DateTime(DateTime.Now.Year, eventDateTime.Value.Month, eventDateTime.Value.Day);
            return localEventTime.DayOfYear - DateTime.Now.DayOfYear;
        }
    }
}
