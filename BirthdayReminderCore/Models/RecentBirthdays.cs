using System.Collections.Generic;
using System.Collections.ObjectModel;
using BirthdayReminder.Models;
using BirthdayReminder.ViewModels;

namespace BirthdayReminderCore.Models
{
    public class Birthdays
    {
        public List<FriendBirthday> RecentBirthdays { get; set; }
        public List<FriendBirthday> AllBirthdays { get; set; }
        public List<AlphaKeyGroup<FriendBirthday>> AlphaGroupAllBirthdays { get; set; }
        public ObservableCollection<CardEntity> BirthdayCards { get; set; }
    }
}
