using System;

namespace BirthdayReminder.ViewModels
{
    public class UserSettings
    {
        public ApplicationWideOptions ApplicationWide { get; set; }
        public ContactSyncOptions ContactSync { get; set; }
        public ReminderNotificationOptions ReminderNotification { get; set; }
        public BirthdayWishOptions BirthdayWish { get; set; }
        public MiscellaneousOptions Miscellaneous { get; set; }
        public UserProfile UserDetails { get; set; }

        public UserSettings()
        {
            //initialise objects
            this.ApplicationWide = new ApplicationWideOptions();
            this.ContactSync = new ContactSyncOptions();
            this.ReminderNotification = new ReminderNotificationOptions();
            this.BirthdayWish = new BirthdayWishOptions();
            this.Miscellaneous = new MiscellaneousOptions();
            this.UserDetails = new UserProfile();

            //set default properties
            ApplicationWide.Version = "2.0";
            ApplicationWide.Publisher = "Shahzad";
            ApplicationWide.SupportEmail = "winappssupport@outlook.com";
            ContactSync.SyncInterval = SyncIntervalEnum.Monthly;
            ContactSync.FacebookSyncEnabled = true;
            ContactSync.LocalSyncEnabled = false;
            ReminderNotification.SendEmailReminders = true;
            ReminderNotification.LocalNotifications = true;
            ReminderNotification.StartTime = StartTimeEnum.OnTime;
            BirthdayWish.SendAutoEmail = true;
            BirthdayWish.AttachPicture = true;
            BirthdayWish.CustomPicUrl = "http://www.hdwallpapers3d.com/wp-content/uploads/2013/05/birthday_celebrations-normal.jpg";
        }
    }

    public class UserProfile
    {
        public String Email { get; set; }
        public String ContactNumber { get; set; }
        public String Name { get; set; }
        public String FacebookId { get; set; }
        public String ProfilePicUrl { get; set; }
    }

    public class ApplicationWideOptions
    {
        public String Version { get; set; }
        public String Publisher { get; set; }
        public String SupportEmail { get; set; }
    }

    public class ContactSyncOptions
    {
        public SyncIntervalEnum SyncInterval { get; set; }
        public DateTime? LastSync { get; set; }
        public bool? FacebookSyncEnabled { get; set; }
        public bool? LocalSyncEnabled { get; set; }
        public bool? CardsSynced { get; set; }
    }

    public class ReminderNotificationOptions
    {
        public bool? SendEmailReminders { get; set; }
        public bool? LocalNotifications { get; set; }
        public StartTimeEnum StartTime { get; set; }
        public TimeUnitEnum TimeUnit { get; set; }
        public int? TimeDuration { get; set; }
    }

    public class BirthdayWishOptions
    {
        public bool? SendAutoEmail { get; set; }
        public bool? AttachPicture { get; set; }
        public String CustomMessage { get; set; }
        public String CustomPicUrl { get; set; }
    }

    public class MiscellaneousOptions
    {

    }

    public enum SyncIntervalEnum
    {
        Daily=0,
        Weekly=1,
        Monthly=2,
        Manual=3
    }

    public enum StartTimeEnum
    {
        Before = 0,
        OnTime = 1,
        After = 2
    }

    public enum TimeUnitEnum
    {
        Hours=0,
        Minutes=1,
        Seconds=2
    }

}
