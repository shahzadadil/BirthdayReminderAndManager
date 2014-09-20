using BirthdayReminder.Resources;
using BirthdayReminder.ViewModels;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace BirthdayReminder
{
    public partial class Settings : PhoneApplicationPage
    {
        readonly Dictionary<SyncIntervalEnum, String> SyncInterval = new Dictionary<SyncIntervalEnum, String>();
        readonly Dictionary<StartTimeEnum, String> ReminderStart = new Dictionary<StartTimeEnum, String>();
        readonly Dictionary<TimeUnitEnum, String> TimeUnit = new Dictionary<TimeUnitEnum, String>();

        public Settings()
        {
            InitializeComponent();
            DataContext = App.UserPreferences;
            Loaded += Settings_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (NavigationService.BackStack.Any(p => p.Source.ToString().Contains("FacebookLogin.xaml")))
                {
                    NavigationService.RemoveBackEntry();
                }
                if (NavigationService.BackStack.First().Source.ToString().Contains("SelectCard.xaml"))
                {
                    SettingsPivot.SelectedItem = BirthdayPivot;
                    NavigationService.RemoveBackEntry();
                }
                if (NavigationService.BackStack.First().Source.ToString().Contains("Settings.xaml"))
                {
                    SettingsPivot.SelectedItem = BirthdayPivot;
                    NavigationService.RemoveBackEntry();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrNavigatingSettings, MessageBoxButton.OK);
            }
        }

        void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadSettingsData();
                LoadAds();

                //display FB log in or log out button
                if (App.FacebookSessionClient == null || App.IsAuthenticated == false)
                {
                    LogIn.Visibility = Visibility.Visible;
                    LogOut.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LogOut.Visibility = Visibility.Visible;
                    LogIn.Visibility = Visibility.Collapsed;
                }

                var saveButton = new ApplicationBarIconButton(new Uri("/Assets/Images/save.png", UriKind.Relative))
                {
                    Text = AppResources.SaveLabel
                };

                saveButton.Click += SaveSettingsClick;

                var appBar = new ApplicationBar {IsVisible = true, Opacity = 0.6};
                appBar.Buttons.Add(saveButton);

                ApplicationBar = appBar;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrAppBarLoad, MessageBoxButton.OK);
            }
        }

        private void LoadAds()
        {
            //ad on about page
            var aboutAd = new AdControl(Constants.StoreAppId, "MainPageBottomAd", true)
            {
                Height = 50, 
                Width = 350
            };
            AboutAdControl.Children.Add(aboutAd);

            //ad on reminder settings page
            var reminderSettingsAd = new AdControl(Constants.StoreAppId, "MainPageBottomAd", true)
            {
                Height = 50,
                Width = 350
            };
            ReminderSettingsAdControl.Children.Add(reminderSettingsAd);

            var wishesAd = new AdControl(Constants.StoreAppId, "MainPageBottomAd", true)
            {
                Height = 50,
                Width = 350
            };
            WishesAdControl.Children.Add(wishesAd);

            var profileAd = new AdControl(Constants.StoreAppId, "MainPageBottomAd", true)
            {
                Height = 50,
                Width = 350
            };
            ProfileAdControl.Children.Add(profileAd);
        }

        private void LoadSettingsData()
        {
            //contact sync pivot
            PopulateSyncInterval();

            //reminder time data                
            PopulateStartTime();

            //start time unit
            PopulateTimeUnit();

            EnableDisableReminderTime(App.UserPreferences.ReminderNotification.StartTime.ToString());
        }

        /// <summary>
        /// Display localized time unit list
        /// </summary>
        private void PopulateTimeUnit()
        {
            StartTimeUnit.Items.Clear();
            StartTimeUnit.Items.Add(AppResources.SyncIntrvlOptionHour);
            StartTimeUnit.Items.Add(AppResources.SyncIntrvlOptionMin);
            StartTimeUnit.Items.Add(AppResources.SyncIntrvlOptionSec);

            TimeUnit.Clear();
            TimeUnit.Add(TimeUnitEnum.Hours, AppResources.SyncIntrvlOptionHour);
            TimeUnit.Add(TimeUnitEnum.Minutes, AppResources.SyncIntrvlOptionMin);
            TimeUnit.Add(TimeUnitEnum.Seconds, AppResources.SyncIntrvlOptionSec);

            StartTimeUnit.SelectedItem = TimeUnit[App.UserPreferences.ReminderNotification.TimeUnit];
        }

        /// <summary>
        /// Display localized reminder start time list
        /// </summary>
        private void PopulateStartTime()
        {
            ReminderStartTime.Items.Clear();
            ReminderStartTime.Items.Add(AppResources.SyncIntrvlOptionBefore);
            ReminderStartTime.Items.Add(AppResources.SyncIntrvlOptionOnTime);
            ReminderStartTime.Items.Add(AppResources.SyncIntrvlOptionAfter);

            ReminderStart.Clear();
            ReminderStart.Add(StartTimeEnum.Before, AppResources.SyncIntrvlOptionBefore);
            ReminderStart.Add(StartTimeEnum.OnTime, AppResources.SyncIntrvlOptionOnTime);
            ReminderStart.Add(StartTimeEnum.After, AppResources.SyncIntrvlOptionAfter);

            ReminderStartTime.SelectedItem = ReminderStart[App.UserPreferences.ReminderNotification.StartTime];
        }

        /// <summary>
        /// Build a localized list for Sync Interval
        /// </summary>
        private void PopulateSyncInterval()
        {
            //add localized options to list
            SyncIntervalList.Items.Clear();
            SyncIntervalList.Items.Add(AppResources.SyncIntrvlOptionDaily);
            SyncIntervalList.Items.Add(AppResources.SyncIntrvlOptionWeekly);
            SyncIntervalList.Items.Add(AppResources.SyncIntrvlOptionMonthly);
            SyncIntervalList.Items.Add(AppResources.SyncIntrvlOptionManual);

            //add options to dictionary
            SyncInterval.Clear();
            SyncInterval.Add(SyncIntervalEnum.Daily, AppResources.SyncIntrvlOptionDaily);
            SyncInterval.Add(SyncIntervalEnum.Weekly, AppResources.SyncIntrvlOptionWeekly);
            SyncInterval.Add(SyncIntervalEnum.Monthly, AppResources.SyncIntrvlOptionMonthly);
            SyncInterval.Add(SyncIntervalEnum.Manual, AppResources.SyncIntrvlOptionManual);

            SyncIntervalList.SelectedItem = SyncInterval[App.UserPreferences.ContactSync.SyncInterval];
        }        

        #region Toggle Buttons

        private void FacebookSyncToggleChecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void FacebookSyncToggleUnchecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void LocalSyncToggleUnchecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void LocalSyncToggleChecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void EmailReminderToggleChecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void EmailReminderToggleUnchecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void LocaltoastToggleUnchecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void LocaltoastToggleChecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void EmailWishToggleChecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void EmailWishToggleUnchecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void SendPicToggleChecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        private void SendPicToggleUnchecked(object sender, RoutedEventArgs e)
        {
            ShowAppBar();
        }

        #endregion

        #region Text Boxes

        private void SyncIntervalList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAppBar();
        }

        private void ReminderStartTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAppBar();

            if (sender == null) return;

            var reminderStart = sender as ListPicker;
            if (reminderStart != null)
                EnableDisableReminderTime((reminderStart.SelectedItem==null) ? String.Empty : reminderStart.SelectedItem.ToString());
        }

        private void StartTimeUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAppBar();
        }

        private void TimeDurationTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowAppBar();
        }

        private void BirthdayMessageTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowAppBar();
        }

        private void NameTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowAppBar();
        }

        private void ContactTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowAppBar();
        }

        private void EmailTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowAppBar();
        }

        #endregion

        #region Button click

        private void RateAppClick(object sender, RoutedEventArgs e)
        {
            var review = new MarketplaceReviewTask();
            review.Show();
        }

        private void SendEmailReviewClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var emailTask = new EmailComposeTask
                {
                    To = App.UserPreferences.ApplicationWide.SupportEmail,
                    Subject = "Birthday Reminder suggestion/query/issue"
                };
                emailTask.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSendingEmail, MessageBoxButton.OK);
            }
        }

        private void SyncNowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.IsTrial)
                {
                    //messagebox to prompt to buy
                    var buyAppForSyncMessageBox = new CustomMessageBox
                    {
                        Height = 300,
                        Caption = AppResources.BuyFullVersion,
                        Message = AppResources.BuyAppGenericMessage,
                        LeftButtonContent = AppResources.BuyLabel,
                        RightButtonContent = AppResources.LaterLabel,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    buyAppForSyncMessageBox.Dismissed += BuyAppForSyncBoxDismissed;
                    buyAppForSyncMessageBox.Show();

                    return;
                }

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    var result = MessageBox.Show(AppResources.ContactSyncRedirectMsg, AppResources.SyncContactsLabel, MessageBoxButton.OKCancel);
                    if (result.Equals(MessageBoxResult.OK))
                    {
                        NavigationService.Navigate(new Uri("/FacebookLogin.xaml?" + UriParameter.IsSyncScneario + "=Yes", UriKind.Relative));
                    }
                }
                else
                {
                    MessageBox.Show(AppResources.WarnInternetMsg, AppResources.WarnNwTitle, MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrLoadSyncScreen, MessageBoxButton.OK);
            }
        }

        private void BuyAppForSyncBoxDismissed(object sender, DismissedEventArgs e)
        {
            if (!e.Result.Equals(CustomMessageBoxResult.LeftButton)) return;

            var buyAppTask = new MarketplaceDetailTask();
            buyAppTask.Show();
        }

        private void SaveSettingsClick(object sender, EventArgs e)
        {
            try
            {
                if (App.IsTrial)
                {
                    //messagebox to prompt to buy
                    var buyAppForSaveSettingsMessageBox = new CustomMessageBox
                    {
                        Height = 300,
                        Caption = AppResources.BuyFullVersion,
                        Message = AppResources.BuyAppSaveSettings,
                        LeftButtonContent = AppResources.BuyLabel,
                        RightButtonContent = AppResources.LaterLabel,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    buyAppForSaveSettingsMessageBox.Dismissed += BuyAppSaveSettingsBoxDismissed;
                    buyAppForSaveSettingsMessageBox.Show();

                    return;
                }

                PerformValiations();

                //Contact
                App.UserPreferences.ContactSync.SyncInterval = SyncInterval.FirstOrDefault(x => x.Value == SyncIntervalList.SelectedItem.ToString()).Key;
                App.UserPreferences.ContactSync.FacebookSyncEnabled = FacebookSyncToggle.IsChecked.HasValue && FacebookSyncToggle.IsChecked.Value;
                App.UserPreferences.ContactSync.LocalSyncEnabled = LocalSyncToggle.IsChecked.HasValue && LocalSyncToggle.IsChecked.Value;

                //Reminder
                App.UserPreferences.ReminderNotification.LocalNotifications = LocalToastToggle.IsChecked.HasValue && LocalToastToggle.IsChecked.Value;
                App.UserPreferences.ReminderNotification.SendEmailReminders = EmailReminderToggle.IsChecked.HasValue && EmailReminderToggle.IsChecked.Value;
                App.UserPreferences.ReminderNotification.StartTime = ReminderStart.FirstOrDefault(x => x.Value == ReminderStartTime.SelectedItem.ToString()).Key;
                App.UserPreferences.ReminderNotification.TimeUnit = TimeUnit.FirstOrDefault(x => x.Value == StartTimeUnit.SelectedItem.ToString()).Key;
                App.UserPreferences.ReminderNotification.TimeDuration = (!string.IsNullOrEmpty(TimeDurationText.Text)) ? Convert.ToInt32(TimeDurationText.Text) : 0;

                //Birthday wish
                App.UserPreferences.BirthdayWish.AttachPicture = SendPicToggle.IsChecked.HasValue && SendPicToggle.IsChecked.Value;
                App.UserPreferences.BirthdayWish.SendAutoEmail = EmailWishToggle.IsChecked.HasValue && EmailWishToggle.IsChecked.Value;
                App.UserPreferences.BirthdayWish.CustomMessage = BirthdayMessageText.Text;

                //Profile
                App.UserPreferences.UserDetails.Name = NameText.Text;
                App.UserPreferences.UserDetails.Email = EmailText.Text;
                App.UserPreferences.UserDetails.ContactNumber = ContactText.Text;

                SettingsUtility.updateUserSettings(App.UserPreferences);

                MessageBox.Show(AppResources.SettingsUpdateSuccessMsg, AppResources.SettingsSavedLabel, MessageBoxButton.OK);

                Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSaveSettings, MessageBoxButton.OK);
            }
        }

        private void BuyAppSaveSettingsBoxDismissed(object sender, DismissedEventArgs e)
        {
            if (!e.Result.Equals(CustomMessageBoxResult.LeftButton)) return;

            var buyAppTask = new MarketplaceDetailTask();
            buyAppTask.Show();
        }

        #endregion        

        private void PerformValiations()
        {
            ValidateTimeDuration();
        }

        private void ShowAppBar()
        {
            try
            {
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrAppBarDisplay, MessageBoxButton.OK);
            }
        }

        private void EnableDisableReminderTime(String startTime)
        {
            if (startTime.Equals(AppResources.SyncIntrvlOptionOnTime,StringComparison.InvariantCultureIgnoreCase))
            {
                StartTimeUnit.IsEnabled = false;
                TimeDurationText.IsEnabled = false;
            }
            else
            {
                StartTimeUnit.IsEnabled = true;
                TimeDurationText.IsEnabled = true;
            }
        }

        private void ValidateTimeDuration()
        {
            var message = String.Empty;

            if (!ReminderStartTime.SelectedItem.ToString().Equals(AppResources.SyncIntrvlOptionOnTime, StringComparison.InvariantCultureIgnoreCase))
            {
                Int32 duration = Convert.ToInt32(TimeDurationText.Text);
                if (StartTimeUnit.SelectedItem.ToString().Equals(AppResources.SyncIntrvlOptionHour, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (duration < 0 || duration > 23)
                    {
                        message = AppResources.WarnValidHourValue;
                    }
                }
                else if (StartTimeUnit.SelectedItem.ToString().Equals(AppResources.SyncIntrvlOptionMin, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (duration < 0 || duration > 59)
                    {
                        message = AppResources.WarnValidMinValue;
                    }
                }
                else if (StartTimeUnit.SelectedItem.ToString().Equals(AppResources.SyncIntrvlOptionSec, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (duration < 0 || duration > 59)
                    {
                        message = AppResources.WarnValidSecvalue;
                    }
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        private void FacebookLogOut(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (App.FacebookSessionClient != null && App.FacebookSessionClient.CurrentSession != null)
                    {
                        App.FacebookSessionClient.Logout();
                    }
                    App.IsAuthenticated = false;

                    LogOut.Visibility = Visibility.Collapsed;
                    LogIn.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show(AppResources.WarnInternetMsg, AppResources.WarnNwTitle, MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrFbLogout, MessageBoxButton.OK);
            }
        }

        private void FacebookLogIn(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show(AppResources.WarnInternetMsg, AppResources.WarnNwTitle, MessageBoxButton.OK);
                }
                else
                {
                    NavigationService.Navigate(new Uri("/FacebookLogin.xaml?" + UriParameter.ReferrerPage + "=Settings.xaml", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrFbLogin, MessageBoxButton.OK);
            }
        }        

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/SelectCard.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrOnNavigation, MessageBoxButton.OK);
            }
        }

        private void BuyAppClick(object sender, RoutedEventArgs e)
        {
            var buyApptask = new MarketplaceDetailTask();
            buyApptask.Show();
        }
    }
}