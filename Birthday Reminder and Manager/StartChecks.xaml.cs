using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Navigation;
using BirthdayReminderCore.Models;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using BirthdayReminder.ViewModels;
using BirthdayReminder.Utilities;
using BirthdayReminder.Resources;
using System.Xml.Serialization;
using BirthdayReminderAgent;
using Microsoft.Phone.Reactive;
using BirthdayReminderCore.Utilities;

namespace BirthdayReminder
{
    public partial class StartChecks
    {
        Boolean _isSourceToast;

        public StartChecks()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                string parameter;
                if (NavigationContext.QueryString.TryGetValue(UriParameter.Action, out parameter))
                    _isSourceToast = true;

                if (!String.IsNullOrEmpty(App.AppLaunchError))
                {
                    throw new Exception(App.AppLaunchError);
                }
                Scheduler.Dispatcher.Schedule(InitialiseApp, TimeSpan.FromSeconds(1));  
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrOpeningApp, MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Loads necessary data and perfor initialisation checks
        /// </summary>
        private void InitialiseApp()
        {
            try
            {
                SetLicenseInfo();
                var isDbUpgraded = AppUtility.UpdateSchema();

                if (!isDbUpgraded)
                {
                    AppUtility.MigrateXmlDataToDatabase();
                    ReminderUtility.ClearAllRemindersOnUpgrade();
                    BirthdayUtility.SaveLocalCards();
                }
                
                UpdateLocalizedRes();
                
                CheckFriendBirthdayFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppResources.ErrAppInitialise, AppResources.ErrInitialiseTitle, MessageBoxButton.OK);
                AppLog.WriteToLog(DateTime.Now, "Error during initialisation. Error : " + ex.Message + ". Stack : " + ex.StackTrace, LogLevel.Error);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void SetLicenseInfo()
        {
            //#if DEBUG

            //App.IsTrial = MessageBox.Show("Use app as trial ?", "Trial or Full", MessageBoxButton.OKCancel) == MessageBoxResult.OK;

            //#endif  
        }

        /// <summary>
        /// Updates the resource with current language to be used by Agent
        /// </summary>
        private static void UpdateLocalizedRes()
        {
            IsolatedStorageFileStream stream = null;
            try
            {
                var fileStore = IsolatedStorageFile.GetUserStoreForApplication();

                if (fileStore.FileExists(FileSystem.ResourceFile))
                {
                    return;
                }

                stream = fileStore.OpenFile(FileSystem.ResourceFile, System.IO.FileMode.Create);
                var serializer = new XmlSerializer(typeof(LocalizedResources));

                var resource = new LocalizedResources
                {
                    BdayTileLabel = AppResources.BirthdayLabel,
                    BdayTodayMsg = AppResources.BirthdayReminderMessage,
                    UpdateBdayReminder = AppResources.UpdateBdayRemindr
                };

                serializer.Serialize(stream, resource);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Populate fb friends if not yet populated
        /// </summary>
        private void CheckFriendBirthdayFile()
        {
            int recordCount;

            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                recordCount = context.Friends.Count();
            }

            if (recordCount == 0)
            {
                var setupContactsMessageBox = new CustomMessageBox
                {
                    Height = 300,
                    Caption = AppResources.BdaySetupTitle,
                    Message = AppResources.BdaySetupMessage,
                    LeftButtonContent = AppResources.YesLabel,
                    RightButtonContent = AppResources.LaterLabel,
                    VerticalAlignment = VerticalAlignment.Center
                };

                setupContactsMessageBox.Dismissed += CustomMessageBoxDismissed;
                setupContactsMessageBox.Show();
            }
            else
            {
                ReminderUtility.UpdateCalendarEntries();                    

                if (_isSourceToast)
                {
                    MessageBox.Show(AppResources.ReminderUpdateSuccess, AppResources.ReminderUpdatedTitle, MessageBoxButton.OK);
                }
                if (IsSyncRequired())
                {
                    var result = MessageBox.Show(AppResources.ContactNotUpdatedMessage, AppResources.SyncContactsLabel, MessageBoxButton.OKCancel);
                    NavigationService.Navigate(result.Equals(MessageBoxResult.OK)
                        ? new Uri("/FacebookLogin.xaml?" + UriParameter.IsSyncScneario + "=Yes", UriKind.Relative)
                        : new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));                        
                }
            }
        }

        /// <summary>
        /// Checks to see if sync is required
        /// </summary>
        /// <returns>Boolean value indicating if sync is required</returns>
        private static bool IsSyncRequired()
        {
            if (!App.UserPreferences.ContactSync.LastSync.HasValue) return false;

            var timeDiff = DateTime.Now.Subtract(App.UserPreferences.ContactSync.LastSync.Value);

            if (App.UserPreferences.ContactSync.SyncInterval.Equals(SyncIntervalEnum.Manual)) return false;

            if (App.UserPreferences.ContactSync.SyncInterval.Equals(SyncIntervalEnum.Daily) && timeDiff.Days > 1)
            {
                return true;
            }
            if (App.UserPreferences.ContactSync.SyncInterval.Equals(SyncIntervalEnum.Weekly) && timeDiff.Days > 7)
            {
                return true;
            }

            return (App.UserPreferences.ContactSync.SyncInterval.Equals(SyncIntervalEnum.Monthly) && timeDiff.Days > 30);
        }

        private void CustomMessageBoxDismissed(object sender, DismissedEventArgs e)
        {
            try
            {
                if (e.Result.Equals(CustomMessageBoxResult.LeftButton))
                {
                    //check if the internet is working, go to Login page. Else display message and load main page
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        NavigationService.Navigate(new Uri("/FacebookLogin.xaml?" + UriParameter.IsSyncScneario + "=No", UriKind.Relative));
                    }
                    else
                    {
                        MessageBox.Show(AppResources.WarnInternetMsg, AppResources.WarnNwTitle, MessageBoxButton.OK);
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    }
                }
                else
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrOnNavigation, MessageBoxButton.OK);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }
    }
}