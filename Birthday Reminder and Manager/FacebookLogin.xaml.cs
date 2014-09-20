using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using BirthdayReminderCore.Models;
using Facebook.Client;
using System.Threading.Tasks;
using BirthdayReminder.ViewModels;
using BirthdayReminder.Resources;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace BirthdayReminder
{
    public partial class FacebookLogin
    {
        private FacebookSession Session;
        private String ReferrerPage = string.Empty;
        private String IsSync = "No";

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationService.BackStack.Any(p => p.Source.ToString().Contains("StartChecks.xaml")))
            {
                NavigationService.RemoveBackEntry();
            }
            if (NavigationService.BackStack.Any(p => p.Source.ToString().Contains("Settings.xaml")))
            {
                NavigationService.RemoveBackEntry();
            }

            NavigationContext.QueryString.TryGetValue(UriParameter.ReferrerPage, out ReferrerPage);
            NavigationContext.QueryString.TryGetValue(UriParameter.IsSyncScneario, out IsSync);
        }

        public FacebookLogin()
        {
            InitializeComponent();
            Loaded += FacebookLogin_Loaded;
        }

        async void FacebookLogin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!App.IsAuthenticated || String.IsNullOrEmpty(App.AccessToken))
                {
                    App.IsAuthenticated = true;
                    await Authenticate();
                }
                else if (!string.IsNullOrEmpty(ReferrerPage))
                {
                    NavigationService.Navigate(new Uri("/" + ReferrerPage, UriKind.Relative));
                }
                else if (!String.IsNullOrEmpty(App.AccessToken))
                {
                    NavigationService.Navigate(new Uri("/FacebookConnect.xaml?" + UriParameter.IsSyncScneario + "=" + IsSync, UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteToLog(DateTime.Now, "Login loading failed !  Error : " + ex.Message + ". Stack : " + ex.StackTrace, LogLevel.Error);
                MessageBox.Show(AppResources.FbLoginFailedText, AppResources.FbLoginFailedTitle, MessageBoxButton.OK);
            }
        }

        private async Task Authenticate()
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    NavigationService.Navigate(!string.IsNullOrEmpty(ReferrerPage)
                        ? new Uri("/" + ReferrerPage, UriKind.Relative)
                        : new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else
                {
                    App.FacebookSessionClient = new FacebookSessionClient(Constants.AppId);
                    Session = await App.FacebookSessionClient.LoginAsync("friends_birthday,friends_photos,email,user_mobile_phone,user_friends,friends_about_me,friends_status,read_friendlists");
                    App.AccessToken = Session.AccessToken;
                    App.FacebookId = Session.FacebookId;

                    if (!string.IsNullOrEmpty(ReferrerPage))
                    {
                        Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/" + ReferrerPage, UriKind.Relative)));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/FacebookConnect.xaml?" + UriParameter.IsSyncScneario + "=" + IsSync, UriKind.Relative)));
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                AppLog.WriteToLog(DateTime.Now, "Login failed !  Error : " + e.Message + ". Stack : " + e.StackTrace, LogLevel.Error);
                MessageBox.Show(AppResources.FbLoginFailedText, AppResources.FbLoginFailedTitle, MessageBoxButton.OK);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            catch(Exception ex)
            {
                AppLog.WriteToLog(DateTime.Now, "Login failed !  Error : " + ex.Message + ". Stack : " + ex.StackTrace, LogLevel.Error);
                MessageBox.Show(AppResources.FbLoginFailedText, AppResources.FbLoginFailedTitle, MessageBoxButton.OK);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }
    }
}