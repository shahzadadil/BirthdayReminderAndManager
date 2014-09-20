using BirthdayReminder.Resources;
using BirthdayReminder.ViewModels;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Phone.Scheduler;
using System;
using System.Linq;

namespace BirthdayReminder.Utilities
{
    public static class ReminderUtility
    {
        /// <summary>
        /// Delete old calendar entries and add new ones
        /// </summary>
        public static void UpdateCalendarEntries()
        {
            try
            {
                ClearOldReminders();

                //add a dummy reminder to test in debug mode
//#if DEBUG

//                if (ScheduledActionService.Find("DummyReminder") != null)
//                {
//                    ScheduledActionService.Remove("DummyReminder");
//                }
//                Reminder dummyReminder = new Reminder("DummyReminder");
//                dummyReminder.BeginTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute + 1, 0);
//                dummyReminder.Title = AppResources.BirthdayLabel;
//                dummyReminder.Content = "Shahzad " + AppResources.BirthdayReminderMessage;
//                ScheduledActionService.Add(dummyReminder);

//#endif              

                

                //create new reminders
                var allBirthdays = BirthdayUtility.GetBirthdays().AllBirthdays;

                if (allBirthdays == null || !allBirthdays.Any()) return;

                var orderedBirthdayList = allBirthdays.Where(b => b.DaysAhead.HasValue && b.DaysAhead > 0)
                    .OrderBy(b => b.DaysAhead)
                    .ToList();

                foreach (var birthday in orderedBirthdayList)
                {
                    //check to see if the reminder aleady exists and the birthday has a value
                    if (ScheduledActionService.Find(birthday.Id.ToString()) != null || !birthday.Birthday.HasValue)
                        continue;

                    //add a reminder
                    var birthdayValue = birthday.Birthday.Value;
                    var reminder = new Reminder(birthday.Id.ToString())
                    {
                        BeginTime = GetBeginTime(birthdayValue),
                        Content = birthday.Name + " " + AppResources.BirthdayReminderMessage,
                        NavigationUri = new Uri("/FriendDetails.xaml?" + UriParameter.FriendId + "=" + birthday.Id,UriKind.Relative),
                        Title = AppResources.BirthdayLabel
                    };

                    ScheduledActionService.Add(reminder);

                    //update friend details to reflect reminder created
                    var friend = BirthdayUtility.GetFriendDetailsById(birthday.Id);

                    if (friend == null) continue;

                    friend.IsReminderCreated = true;
                    BirthdayUtility.UpdateFriendDetails(friend);
                }
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Contains("The maximum number of ScheduledActions of this type have already been added"))
                {
                    throw;
                }
            }
        }

        private static void ClearOldReminders()
        {
            var reminders = ScheduledActionService.GetActions<Reminder>().ToArray();

            //delete reminders
            if (reminders.Length > 0)
            {
                foreach (var reminder in reminders)
                {
                    if (ScheduledActionService.Find(reminder.Name) != null)
                    {
                        ScheduledActionService.Remove(reminder.Name);
                    }

                    var friend = BirthdayUtility.GetFriendDetailsById(Convert.ToInt32(reminder.Name));

                    if (friend == null) continue;

                    friend.IsReminderCreated = false;
                    BirthdayUtility.UpdateFriendDetails(friend);

                    //AppLog.writeToLog(DateTime.Now, reminder.Name + friend.UniqueId + friend.IsReminderCreated.ToString(), LogLevel.Error);
                }
            }
        }

        public static void ClearAllRemindersOnUpgrade()
        {
            var reminders = ScheduledActionService.GetActions<Reminder>().ToArray();

            //delete reminders
            if (reminders.Length <= 0) return;

            foreach (var reminder in reminders.Where(reminder => ScheduledActionService.Find(reminder.Name) != null))
            {
                ScheduledActionService.Remove(reminder.Name);
            }
        }

        /// <summary>
        /// Calculate the begin time of the reminder
        /// </summary>
        /// <param name="birthdayValue">The date of the birthday</param>
        /// <returns>Date and time for the reminder start</returns>
        private static DateTime GetBeginTime(DateTime birthdayValue)
        {
            var birthday = new DateTime(DateTime.Now.Year, birthdayValue.Month, birthdayValue.Day);
            var newTime = birthday;
            if (!App.UserPreferences.ReminderNotification.StartTime.Equals(StartTimeEnum.OnTime))
            {
                //calculate time difference
                TimeSpan timeSpan;
                if (App.UserPreferences.ReminderNotification.TimeDuration.HasValue)
                {
                    if (App.UserPreferences.ReminderNotification.TimeUnit.Equals(TimeUnitEnum.Hours))
                    {
                        timeSpan = new TimeSpan(App.UserPreferences.ReminderNotification.TimeDuration.Value, 0, 0);
                    }
                    else if (App.UserPreferences.ReminderNotification.TimeUnit.Equals(TimeUnitEnum.Minutes))
                    {
                        timeSpan = new TimeSpan(0, App.UserPreferences.ReminderNotification.TimeDuration.Value, 0);
                    }
                    else if (App.UserPreferences.ReminderNotification.TimeUnit.Equals(TimeUnitEnum.Seconds))
                    {
                        timeSpan = new TimeSpan(0, 0, App.UserPreferences.ReminderNotification.TimeDuration.Value);
                    }
                }
                else
                {
                    timeSpan = new TimeSpan(0, 0, 0);
                }

                //formulate new time
                if (App.UserPreferences.ReminderNotification.StartTime.Equals(StartTimeEnum.Before))
                {
                    newTime = birthday.Subtract(timeSpan);
                }
                else if (App.UserPreferences.ReminderNotification.StartTime.Equals(StartTimeEnum.After))
                {
                    newTime = birthday.Add(timeSpan);
                }

                return newTime;
            }

            return birthday;
        }
    }
}
