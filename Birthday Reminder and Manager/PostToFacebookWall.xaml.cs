using BirthdayReminder.Resources;
using BirthdayReminderCore.Models;
using Microsoft.Phone.Controls;
using System;
using System.Text;
using System.Windows;
using System.Windows.Navigation;

namespace BirthdayReminder
{
    public partial class PostToFacebookWall : PhoneApplicationPage
    {
        string FriendId = string.Empty;

        public PostToFacebookWall()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            try
            {
                if (NavigationContext.QueryString.TryGetValue(UriParameter.FriendId, out FriendId))
                {
                    var queryString = new StringBuilder(UriParameter.FeedDialogUrl);
                    queryString.Append("?");
                    queryString.Append(UriParameter.FacebookAppId + Constants.AppId);
                    queryString.Append(Constants.UrlParamaterSeparator);
                    //queryString.Append(UriParameter.FeedDisplayType + "popup");
                    //queryString.Append(Constants.UrlParamaterSeparator);
                    queryString.Append(UriParameter.FeedCaption + AppResources.HappyBdayMessage);
                    queryString.Append(Constants.UrlParamaterSeparator);
                    queryString.Append(UriParameter.FeedTo + FriendId);
                    queryString.Append(Constants.UrlParamaterSeparator);

                    if (App.UserPreferences!=null && App.UserPreferences.BirthdayWish!=null && App.UserPreferences.BirthdayWish.AttachPicture.HasValue 
                        && App.UserPreferences.BirthdayWish.AttachPicture.Value && !String.IsNullOrEmpty(App.UserPreferences.BirthdayWish.CustomPicUrl))
                    {
                        queryString.Append(UriParameter.FeedPicture + App.UserPreferences.BirthdayWish.CustomPicUrl);
                        queryString.Append(Constants.UrlParamaterSeparator);
                    }

                    queryString.Append(UriParameter.FeedDescription + (App.UserPreferences != null && (App.UserPreferences.BirthdayWish !=null) ? App.UserPreferences.BirthdayWish.CustomMessage : String.Empty));
                    queryString.Append(Constants.UrlParamaterSeparator);
                    queryString.Append(UriParameter.FeedRedirectUri + "http://facebook.com/");

                    BrowserControl.Source = new Uri(queryString.ToString(),UriKind.Absolute);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrOnNavigation, MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }

        private void PhoneApplicationPageLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void BrowserControlLoadCompleted(object sender, NavigationEventArgs e)
        {
            StatusPanel.Visibility = Visibility.Collapsed;
        }

        void BrowserControlNavigating(object sender, NavigatingEventArgs e)
        {
            try
            {
                if (e.Uri.Host.Contains("m.facebook.com") && e.Uri.ToString().Contains("post_id") && e.Uri.ToString().Contains("_rdr"))
                {
                    String friendGuid; 
                    NavigationContext.QueryString.TryGetValue(UriParameter.FriendGuid, out friendGuid);
                    MessageBox.Show(AppResources.MsgPostedFb);
                    NavigationService.Navigate(new Uri("/FriendDetails.xaml?" + UriParameter.FriendId + "=" + friendGuid, UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrOnNavigation, MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }
    }
}