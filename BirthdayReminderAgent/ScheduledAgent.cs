using System.Diagnostics;
using System.Windows;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Phone.Scheduler;
using System.Collections.Generic;
using BirthdayReminder.ViewModels;
using System;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace BirthdayReminderAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        UserSettings Settings;
        LocalizedResources Resources;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            try
            {
                //get the list of birthdays today and tomorrow
                GetRecentBirthdays();
                AppLog.CleanUpLogFile();
            }
            catch (Exception ex)
            {
                AppLog.WriteToLog(DateTime.Now, "Ërror in agent : " + ex.Message + ". Stack : " + ex.StackTrace, LogLevel.Error);
            }
            finally
            {
                NotifyComplete();
            }
        }

        /// <summary>
        /// Retrieves list of recent birthdays
        /// </summary>
        /// <returns>List of friend</returns>
        private void GetRecentBirthdays()
        {
            try
            {
                var friendList = BirthdayUtility.GetFriendList();
                var recentBirthdays = new List<FriendEntity>();
                var todayBirthday = new List<FriendEntity>();
                var weekBirthday = new List<FriendEntity>();
                Settings = SettingsUtility.GetUserPreferences();
                LoadLocalizedRes();
                var notificationCount = 0;
                var isReminderExpired = false;
                foreach (var friend in friendList)
                {
                    if (!friend.Birthday.HasValue || (friend.IsHidden.HasValue && friend.IsHidden.Value)) continue;

                    var daysAhead = DateTimeUtility.GetTimeToEvent(friend.Birthday.Value);

                    //if the birthday is today, add recent birthdays to list
                    if (daysAhead == 0)
                    {
                        //if the birthday is today, raise a toast notification
                        recentBirthdays.Add(friend);
                        todayBirthday.Add(friend);
                        notificationCount++;

                        if (friend.LastToastRaisedYear < DateTime.Now.Year)
                        {
                            if (!Settings.ReminderNotification.LocalNotifications.HasValue || Settings.ReminderNotification.LocalNotifications.Value)
                            {
                                RaiseToast(friend.Name + " " + Resources.BdayTodayMsg, Resources.BdayTileLabel, new Uri("/FriendDetails.xaml?" + UriParameter.FriendId + "=" + friend.UniqueId, UriKind.Relative));
                            }

                            //update the last notification raised year
                            friend.LastToastRaisedYear = DateTime.Now.Year;
                            BirthdayUtility.UpdateFriendDetails(friend);
                        }
                    }

                    //if birthday is tomorrow
                    if (daysAhead == 1)
                    {
                        recentBirthdays.Add(friend);
                        notificationCount++;
                    }

                    //check if reminder for upcoming birthdays exist
                    if (daysAhead > 0 && daysAhead < 7)
                    {
                        if (!friend.IsReminderCreated)
                        {
                            isReminderExpired = true;
                            notificationCount++;
                        }
                        weekBirthday.Add(friend);
                    }
                }

                //raise a toast notification if reminders have to be created
                if (isReminderExpired)
                {
                    RaiseToast(Resources.UpdateBdayReminder, Resources.BdayTileLabel, new Uri("/StartChecks.xaml?" + UriParameter.Action + "=Toast", UriKind.Relative));
                }

                //update shell tile
                UpdateTile(notificationCount, weekBirthday);

                BirthdayUtility.DownloadProfileImages("No");

                //send an email - [[to be implemented later]]
                //if (settings.ReminderNotification.SendEmailReminders && !string.IsNullOrEmpty(settings.UserDetails.Email))
                //{
                //    String emailText = getReminderEmailContent(todayBirthday);
                //    sendEmail(settings.UserDetails.Email,emailText);
                //}

                ////wish via email
                //if (todayBirthday!=null && todayBirthday.Count>0)
                //{
                //    wishViaEmail(todayBirthday);
                //}                
            }
            catch (Exception ex)
            {
                AppLog.WriteToLog(DateTime.Now, ex.Message + ". " + ex.StackTrace, LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Reads local resources for display of tile and toast
        /// </summary>
        /// <returns>The object containing lcoal strings</returns>
        private void LoadLocalizedRes()
        {
            IsolatedStorageFileStream stream = null;
            try
            {
                var fileStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (fileStore.FileExists(FileSystem.ResourceFile))
                {
                    var serializer = new XmlSerializer(typeof(LocalizedResources));
                    stream = fileStore.OpenFile(FileSystem.ResourceFile, System.IO.FileMode.Open);
                    Resources = (LocalizedResources)serializer.Deserialize(stream);
                }
                else
                {
                    Resources.BdayTileLabel = "Birthday";
                    Resources.BdayTodayMsg = "'s birthday Today. Tap to send him a message";
                    Resources.UpdateBdayReminder = "Update birthday reminders";
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        private void WishViaEmail(List<Friend> todayBirthday)
        {
        }

        /// <summary>
        /// Get the HTML template for the email
        /// </summary>
        /// <param name="todayBirthday">List of friends having birthday today</param>
        /// <returns></returns>
        private String GetReminderEmailContent(IEnumerable<FriendEntity> todayBirthday)
        {
            var friendList = new StringBuilder();
            foreach (var friend in todayBirthday)
            {
                string friendTemplate = Constants.FriendEmailTemplate;
                friendTemplate = friendTemplate.Replace("[[imagesource]]", friend.ProfilePictureUrl);
                friendTemplate = friendTemplate.Replace("[[profileurl]]", friend.FacebookId);
                friendList.Append(friendTemplate);
            }
            return Constants.ReminderContainerTemplate.Replace("[[friendplaceholder]]", friendList.ToString());
        }

        /// <summary>
        /// Shows a toast notification
        /// </summary>
        /// <param name="content">Content of notification</param>
        /// <param name="title">Title of notification</param>
        /// <param name="navigationUri">URI to navigate to</param>
        private static void RaiseToast(String content, String title, Uri navigationUri)
        {
            var toast = new ShellToast {Content = content, Title = title, NavigationUri = navigationUri};
            toast.Show();
        }

        /// <summary>
        /// Update the app tile
        /// </summary>
        /// <param name="notificationCount">Count of notifications to show</param>
        /// <param name="friendList"></param>
        private void UpdateTile(int notificationCount, IEnumerable<FriendEntity> friendList)
        {
            if (friendList == null) return;

            var tile = ShellTile.ActiveTiles.First();

            if (tile == null) return;

            //create live tile
            var tileData = new CycleTileData
            {
                Title = Resources.BdayTileLabel,
                CycleImages = friendList.Where
                    (
                        (t, i) => i < 9
                    )
                    .Select
                    (
                        t =>
                            new Uri(@"isostore:/" + FileSystem.ProfilePictureDirectory + t.UniqueId + ".jpg",
                                UriKind.Absolute)
                    ).ToArray()
            };

            //build colelction of image urls

            //update notification count
            if (notificationCount != 0)
            {
                tileData.Count = notificationCount;
            }

            //update tile
            tile.Update(tileData);
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailTo">Target email</param>
        /// <param name="message">Email body</param>
        //private void sendEmail(String emailTo, String message)
        //{
        //    try
        //    {
        //        string emailFrom = settings.ApplicationWide.SupportEmail;
        //        EmailServiceClient client = new EmailServiceClient("BasicHttpBinding_IEmailService");
        //        client.sendEmailCompleted += mailSent;
        //        client.CloseCompleted += mailSendingClosed;
        //        client.sendEmailAsync(emailFrom, emailTo, "Happy Birthday !", message);
        //    }
        //    catch (Exception)
        //    {                
        //        throw;
        //    }
        //}

        //private void mailSendingClosed(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Error != null)
        //        {
        //            AppLog.writeToLog(DateTime.Now, e.Error.ToString(), LogLevel.Error);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AppLog.writeToLog(DateTime.Now, ex.Message.ToString() + ex.StackTrace + ex.InnerException, LogLevel.Error);
        //    }
        //}

        //private void mailSent(object sender, sendEmailCompletedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Error!=null)
        //        {
        //            AppLog.writeToLog(DateTime.Now, e.Error.ToString(), LogLevel.Error);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AppLog.writeToLog(DateTime.Now, ex.Message.ToString()+ex.StackTrace+ex.InnerException, LogLevel.Error);
        //    }
        //}
    }
}