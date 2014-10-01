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

            var dayOfYearBirthday = eventDateTime.Value.DayOfYear;
            var currentDayOfYear = DateTime.Today.DayOfYear;

            var nextBirthday = new DateTime(DateTime.Today.Year, eventDateTime.Value.Month, eventDateTime.Value.Day);

            if (dayOfYearBirthday < currentDayOfYear)
            {
                nextBirthday = nextBirthday.AddYears(1);
                return nextBirthday.Subtract(DateTime.Today).Days;
            }

            return nextBirthday.DayOfYear - currentDayOfYear;
        }
    }
}
