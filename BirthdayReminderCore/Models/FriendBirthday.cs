using System;

namespace BirthdayReminderCore.Models
{
    public class FriendBirthday
    {
        public int Id { get; set; }
        public string FacebookId { get; set; }
        public DateTime? Birthday { get; set; }
        public string BirthdayText { get; set; }
        public string Name { get; set; }
        public string TimeToEventText { get; set; }
        public int? DaysAhead { get; set; }
        public string ProfilePicUrl { get; set; }
    }
}
