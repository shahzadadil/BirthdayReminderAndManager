using BirthdayReminder.Resources;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace BirthdayReminder
{
    public partial class FriendDetails : PhoneApplicationPage
    {
        private bool IsSaveButtonEnabled { get; set; }
        private FriendEntity FriendEntity;
        string Parameter = string.Empty;
        private ActionType Action = ActionType.Details;

        public FriendDetails()
        {
            InitializeComponent();

            if (App.IsTrial)
            {
                LoadAds();
            }
            
        }

        private void LoadAds()
        {
            var bottomAd = new AdControl(Constants.StoreAppId, "MainPageBottomAd", true)
            {
                Height = 50,
                Width = 350,
            };
            FriendDetailsAdControl.Children.Add(bottomAd);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (NavigationContext.QueryString.TryGetValue(UriParameter.FriendId, out Parameter))
                {
                    Action = ActionType.Details;
                    FriendEntity = BirthdayUtility.GetFriendDetailsById(Convert.ToInt32(Parameter));
                    DataContext = FriendEntity;
                    var birthday = DateTime.Now;

                    BirthdayText.Text = FriendEntity.Birthday.HasValue
                        ? AppResources.BirthdayOnLabel + " : " + FriendEntity.Birthday.Value.ToString("dd MMM")
                        : AppResources.BirthdayOnLabel + " : " + AppResources.NotKnownLabel;
                    

                    //update the UI based on the action
                    UpdateUIForAction(Action);

                    AutoEmailToggle.IsChecked = FriendEntity.SendAutoEmailOnBirthday;
                    AutoEmailToggle.Content = (FriendEntity.SendAutoEmailOnBirthday) ? "Yes" : "No";

                    //format datetime to current culture
                    BirthdayPicker.Value = new DateTime(birthday.Year, birthday.Month, birthday.Day, CultureInfo.CurrentCulture.Calendar);

                    //add buttons to app bar
                    AddAppBarButtons(FriendEntity);
                }
                else
                {
                    Action = ActionType.Add;
                    ActivateSaveButton();
                }

                //remove backstack            
                while (NavigationService.BackStack.Any() && !NavigationService.BackStack.ElementAt(0).Source.ToString().Contains("/MainPage.xaml"))
                {
                    NavigationService.RemoveBackEntry();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error On Navigation", MessageBoxButton.OK);
            }
        }

        private void UpdateUIForAction(ActionType action)
        {
            if (action.Equals(ActionType.Details))
            {
                NamePanel.Visibility = Visibility.Collapsed;
                PicAndNamePanel.Visibility = Visibility.Visible;
            }
            else if (action.Equals(ActionType.Add))
            {
                PicAndNamePanel.Visibility = Visibility.Collapsed;
                NamePanel.Visibility = Visibility.Visible;
            }
            else if (action.Equals(ActionType.Update))
            {
                PicAndNamePanel.Visibility = Visibility.Visible;
                NamePanel.Visibility = Visibility.Visible;
            }
            else if (action.Equals(ActionType.Delete))
            {
                PicAndNamePanel.Visibility = Visibility.Visible;
                NamePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddAppBarButtons(FriendEntity friendDetails)
        {
            ApplicationBar.Buttons.Clear();
            IsSaveButtonEnabled = false;

            if (friendDetails.TypeOfContact == ContactType.Facebook)
            {
                var postToWallButton = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/Assets/Images/FbButtonIcon_48x48 copy.PNG", UriKind.Relative),
                    Text = AppResources.PostToWall
                };

                postToWallButton.Click += PostToWallButtonClick;
                ApplicationBar.Buttons.Add(postToWallButton);
            }

            if (!string.IsNullOrEmpty(friendDetails.Email))
            {
                var sendEmailButton = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/Assets/Images/EmailButtonIcon_48x48.png", UriKind.Relative),
                    Text = AppResources.EmailLabel
                };

                sendEmailButton.Click += SendEmailButtonClick;
                ApplicationBar.Buttons.Add(sendEmailButton);
            }

            if (!string.IsNullOrEmpty(friendDetails.PhoneNumber))
            {
                var callButton = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/Assets/AppBar/feature.phone.png", UriKind.Relative),
                    Text = AppResources.CallLabel
                };

                callButton.Click += CallButtonClick;
                ApplicationBar.Buttons.Add(callButton);

                var sendMessageButton = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/Assets/AppBar/feature.email.png", UriKind.Relative),
                    Text = AppResources.MessageLabel
                };

                sendMessageButton.Click += SendMessageButtonClick;
                ApplicationBar.Buttons.Add(sendMessageButton);
            }

            ApplicationBar.IsVisible = ApplicationBar.Buttons.Count>0;
            
        }

        private void SendMessageButtonClick(object sender, EventArgs e)
        {
            try
            {
                var friendDetails = DataContext as Friend;

                if (friendDetails == null) return;

                var smsTask = new SmsComposeTask {To = friendDetails.PhoneNumber, Body = AppResources.BirthdayMessage};
                smsTask.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSendingMessage, MessageBoxButton.OK);
            }
        }

        private void CallButtonClick(object sender, EventArgs e)
        {
            try
            {
                var dialTask = new PhoneCallTask();
                var friendDetails = DataContext as FriendEntity;

                if (friendDetails != null) dialTask.PhoneNumber = friendDetails.PhoneNumber;

                dialTask.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Dialling Number", MessageBoxButton.OK);
            }
        }

        private void SendEmailButtonClick(object sender, EventArgs e)
        {
            try
            {
                var friendDetails = DataContext as Friend;

                if (friendDetails == null) return;

                var emailTask = new EmailComposeTask
                {
                    To = friendDetails.Email,
                    Subject = "Happy Birthday",
                    Body = "Many Many Happy Returns Of The Day :)"
                };

                emailTask.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Sending Email", MessageBoxButton.OK);
            }
        }

        void PostToWallButtonClick(object sender, EventArgs e)
        {
            try
            {
                var parameter = UriParameter.FriendId + "=" + FriendEntity.FacebookId + Constants.UrlParamaterSeparator +
                                   UriParameter.FriendGuid + "=" + FriendEntity.UniqueId;
                NavigationService.Navigate(new Uri("/PostToFacebookWall.xaml?" + parameter, UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrPostingToWall, MessageBoxButton.OK);
            }
        }

        private void NameTextChange(object sender, EventArgs e)
        {
            ActivateSaveButton();
        }

        private void EmailTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ActivateSaveButton();
        }

        private void ContactTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ActivateSaveButton();
        }

        private void AutoEmailToggleChecked(object sender, RoutedEventArgs e)
        {
            AutoEmailToggle.Content = "Yes";
            ActivateSaveButton();
        }

        private void AutoEmailToggleUnchecked(object sender, RoutedEventArgs e)
        {
            AutoEmailToggle.Content = "No";
            ActivateSaveButton();
        }

        private void ActivateSaveButton()
        {
            try
            {
                if (IsSaveButtonEnabled || ApplicationBar == null) return;

                ApplicationBar.Buttons.Clear();
                IsSaveButtonEnabled = true;

                var saveButton = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/Assets/Images/save.png", UriKind.Relative),
                    Text = AppResources.SaveLabel
                };
                saveButton.Click += SaveButtonClick;

                ApplicationBar.Buttons.Add(saveButton);
                ApplicationBar.IsVisible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrDisplaSaveBtn, MessageBoxButton.OK);
            }
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (App.IsTrial)
                {
                    //messagebox to prompt to buy
                    var buyAppForSaveFriendMessageBox = new CustomMessageBox
                    {
                        Height = 300,
                        Caption = AppResources.BuyFullVersion,
                        Message = AppResources.BuyAppForAddFriend,
                        LeftButtonContent = AppResources.BuyLabel,
                        RightButtonContent = AppResources.LaterLabel,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    buyAppForSaveFriendMessageBox.Dismissed += BuyAppForSaveFriendBoxDismissed;
                    buyAppForSaveFriendMessageBox.Show();

                    return;
                }
                if (String.IsNullOrEmpty(NameTextBox.Text) || !BirthdayPicker.Value.HasValue)
                {
                    MessageBox.Show(AppResources.WarnNameBdayCompulsory, AppResources.WarnDtlsReq, MessageBoxButton.OK);
                    return;
                }

                //display status message
                StatusText.Text = AppResources.SavingFrndDtls;
                ContentStackPanel.Opacity = 0.2;
                StatusPanel.Visibility = Visibility.Visible;
                int friendGuid;

                if (Action.Equals(ActionType.Add))
                {
                    friendGuid = AddNewFriend();
                    MessageBox.Show(AppResources.NewFrndCreatedMsg, AppResources.NewFrndAddedTitle, MessageBoxButton.OK);
                }
                else
                {
                    friendGuid = UpdateFriend();
                    MessageBox.Show(AppResources.FrndDtlsUpdated, AppResources.FrndDtlsUpdatedTitle, MessageBoxButton.OK);
                }                

                //change the app bar buttons
                FriendEntity = BirthdayUtility.GetFriendDetailsById(friendGuid);
                AddAppBarButtons(FriendEntity);

                //remove focus from textbox
                RemoveFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSavingFrndDtls, MessageBoxButton.OK);
            }
            finally
            {
                //hide status screen
                StatusPanel.Visibility = Visibility.Collapsed;
                ContentStackPanel.Opacity = 1;
            }
        }

        private void BuyAppForSaveFriendBoxDismissed(object sender, DismissedEventArgs e)
        {
            if (!e.Result.Equals(CustomMessageBoxResult.LeftButton)) return;

            var buyAppTask = new MarketplaceDetailTask();
            buyAppTask.Show();
        }

        private int AddNewFriend()
        {
            //gather friend details
            var friendDetails = new FriendEntity
            {
                Name = NameTextBox.Text,
                Birthday =
                    (BirthdayPicker.Value.HasValue)
                        ? BirthdayPicker.Value.Value
                        : new DateTime?(),
                Email = EmailTextBox.Text,
                PhoneNumber = ContactTextBox.Text,
                SendAutoEmailOnBirthday = AutoEmailToggle.IsChecked != null && AutoEmailToggle.IsChecked.Value,
                ProfilePictureUrl = Constants.DefaultProfilePic,
                TypeOfContact = ContactType.Custom
            };

            //write updated details to db
            return BirthdayUtility.AddNewFriend(friendDetails);
        }

        private int UpdateFriend()
        {
            //gather friend details
            var friendDetails = DataContext as FriendEntity;

            if (friendDetails == null) return 0;

            friendDetails.Email = EmailTextBox.Text;
            friendDetails.PhoneNumber = ContactTextBox.Text;
            friendDetails.SendAutoEmailOnBirthday = AutoEmailToggle.IsChecked != null && AutoEmailToggle.IsChecked.Value;
            friendDetails.Name = NameTextBox.Text;
            friendDetails.Birthday = (BirthdayPicker.Value.HasValue) ? BirthdayPicker.Value.Value : new DateTime?();

            //write updated details to file
            BirthdayUtility.UpdateFriendDetails(friendDetails);

            return friendDetails.UniqueId;
        }

        private void RemoveFocus()
        {
            AutoEmailToggle.Focus();
        }

        private void NameTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ActivateSaveButton();
        }

        private void BirthdayPickerValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            ActivateSaveButton();
        }
    }
}