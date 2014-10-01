using BirthdayReminder.Models;
using BirthdayReminder.Resources;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace BirthdayReminder
{
    public partial class MainPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            var addButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/add.png", UriKind.Relative))
            {
                Text = AppResources.AddLabel
            };
            addButton.Click += AddClick;

            var deleteButton = new ApplicationBarIconButton(new Uri("/Assets/Images/delete.png", UriKind.Relative))
            {
                Text = AppResources.DeleteLabel
            };
            deleteButton.Click += DeleteAppBarClick;

            var settingsButton = new ApplicationBarIconButton(new Uri("/Assets/Images/feature.settings.png", UriKind.Relative))
            {
                Text = AppResources.SettingsLabel
            };
            settingsButton.Click += SettingsClick;

            ApplicationBar.Buttons.Add(addButton);
            ApplicationBar.Buttons.Add(deleteButton);
            ApplicationBar.Buttons.Add(settingsButton);
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                var lastSync = App.UserPreferences.ContactSync.LastSync;
                var firstTimeSetup = lastSync.HasValue && lastSync.Value.AddMinutes(2) >= DateTime.Now;

                while (NavigationService.BackStack.Any())
                {
                    NavigationService.RemoveBackEntry();
                }
                if (RecentBirthdayList != null)
                {
                    RecentBirthdayList.SelectedItems.Clear();
                }


                //Launch the agent for test in debug mode only
                //#if DEBUG
                //                ScheduledActionService.LaunchForTest(BirthdayAppInfo.BackgroundAgentName, TimeSpan.FromSeconds(20));
                //#endif


                BindDataContext();

                if (!App.IsTrial || !firstTimeSetup) return;

                var buyAppForDeleteMessageBox = new CustomMessageBox
                {
                    Caption = AppResources.BuyFullVersion,
                    Message = AppResources.BuyAppForAllFriends,
                    LeftButtonContent = AppResources.BuyLabel,
                    RightButtonContent = AppResources.LaterLabel,
                    VerticalAlignment = VerticalAlignment.Center
                };

                buyAppForDeleteMessageBox.Dismissed += BuyAppForAllFriendsMessageBoxDismissed;
                buyAppForDeleteMessageBox.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrBindingData, MessageBoxButton.OK);
            }
        }

        private void BuyAppForAllFriendsMessageBoxDismissed(object sender, DismissedEventArgs e)
        {
            if (!e.Result.Equals(CustomMessageBoxResult.LeftButton)) return;

            var buyAppTask = new MarketplaceDetailTask();
            buyAppTask.Show();
        }

        private void DeleteAppBarClick(object sender, EventArgs e)
        {
            try
            {
                if (App.IsTrial)
                {
                    //messagebox to prompt to buy
                    var buyAppForDeleteMessageBox = new CustomMessageBox
                    {
                        Height = 300,
                        Caption = AppResources.BuyFullVersion,
                        Message = AppResources.BuyAppForDelete,
                        LeftButtonContent = AppResources.BuyLabel,
                        RightButtonContent = AppResources.LaterLabel,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    buyAppForDeleteMessageBox.Dismissed += BuyAppForDeleteMessageBoxDismissed;
                    buyAppForDeleteMessageBox.Show();

                    return;
                }
                //display warning for confirmation
                var result = MessageBox.Show(AppResources.DeleteConfirmMessage, AppResources.DeleteLabel, MessageBoxButton.OKCancel);

                if (!result.Equals(MessageBoxResult.OK)) return;

                //check which list is active
                if (MainPagePanorama.SelectedItem.Equals(MostRecentPanorama))
                {
                    if (RecentBirthdayList != null && RecentBirthdayList.SelectedItems != null && RecentBirthdayList.SelectedItems.Count > 0)
                    {
                        foreach (FriendBirthday friend in RecentBirthdayList.SelectedItems)
                        {
                            BirthdayUtility.DeleteFriend(friend.Id);
                        }
                    }
                    else
                    {
                        MessageBox.Show(AppResources.WarnSelectItem, AppResources.WarnTryAgain, MessageBoxButton.OK);
                    }

                    BindDataContext();
                }
                else if (MainPagePanorama.SelectedItem.Equals(AllItemPanorama))
                {
                    if (AllBirthdayList != null && AllBirthdayList.SelectedItems != null && AllBirthdayList.SelectedItems.Count > 0)
                    {
                        foreach (FriendBirthday friend in AllBirthdayList.SelectedItems)
                        {
                            BirthdayUtility.DeleteFriend(friend.Id);
                        }
                    }
                    else
                    {
                        MessageBox.Show(AppResources.WarnSelectItem, AppResources.WarnTryAgain, MessageBoxButton.OK);
                    }

                    BindDataContext();
                }
                else if (MainPagePanorama.SelectedItem.Equals(BirthdayCardPanorama))
                {
                    //if image upload/delete is complete
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (BirthdayCardList != null && BirthdayCardList.SelectedItems != null && BirthdayCardList.SelectedItems.Count > 0)
                        {
                            var service = new AzureStorageService(Services.AzureConnectionString, App.UserPreferences.UserDetails.FacebookId);
                            var items = BirthdayCardList.SelectedItems;
                            var deletedCards = new List<int>();

                            if (items != null)
                            {
                                foreach (var cardEntity in items.Cast<CardEntity>())
                                {
                                    service.DeleteBlob(Path.GetFileName(cardEntity.Url));
                                    deletedCards.Add(cardEntity.Id);
                                }
                            }

                            var birthdays = DataContext as Birthdays;

                            if (birthdays == null) return;

                            var birthdayCards = birthdays.BirthdayCards;

                            foreach (var cardId in deletedCards)
                            {
                                birthdayCards.Remove(birthdayCards.Single(c => c.Id == cardId));
                            }

                            //remove entry fromm database
                            BirthdayUtility.DeleteCards(deletedCards);
                        }
                        else
                        {
                            MessageBox.Show(AppResources.WarnSelectItem, AppResources.WarnTryAgain, MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show(AppResources.WarnInternetMsg, AppResources.WarnNwTitle, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrDeleting, MessageBoxButton.OK);
            }
        }

        private void BuyAppForDeleteMessageBoxDismissed(object sender, DismissedEventArgs e)
        {
            if (!e.Result.Equals(CustomMessageBoxResult.LeftButton)) return;

            var buyAppTask = new MarketplaceDetailTask();
            buyAppTask.Show();
        }

        private void RecentBirthdayListTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var frameworkElement = sender as FrameworkElement;

                if (frameworkElement == null) return;

                var friend = frameworkElement.DataContext as FriendBirthday;

                if (friend != null)
                {
                    NavigationService.Navigate(new Uri("/FriendDetails.xaml?" + UriParameter.FriendId + "=" + friend.Id, UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSelecting, MessageBoxButton.OK);
            }
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading settings", MessageBoxButton.OK);
            }
        }

        private void AddClick(object sender, EventArgs e)
        {
            try
            {
                //check which list is active
                if (MainPagePanorama.SelectedItem.Equals(MostRecentPanorama) || MainPagePanorama.SelectedItem.Equals(AllItemPanorama))
                {
                    NavigationService.Navigate(new Uri("/FriendDetails.xaml", UriKind.Relative));
                }
                else if (MainPagePanorama.SelectedItem.Equals(BirthdayCardPanorama))
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        var bdayObj = DataContext as Birthdays;

                        if (bdayObj == null) return;

                        if (App.IsTrial)
                        {
                            if (bdayObj.BirthdayCards == null || bdayObj.BirthdayCards.Count < 5)
                            {
                                UploadPhoto();
                            }
                            else
                            {
                                //messagebox to prompt to buy
                                var buyAppMessageBox = new CustomMessageBox
                                {
                                    Height = 300,
                                    Caption = AppResources.BuyFullVersion,
                                    Message = AppResources.BuyAppForCard,
                                    LeftButtonContent = AppResources.BuyLabel,
                                    RightButtonContent = AppResources.LaterLabel,
                                    VerticalAlignment = VerticalAlignment.Center
                                };

                                buyAppMessageBox.Dismissed += BuyAppMessageBoxDismissed;
                                buyAppMessageBox.Show();
                            }
                        }
                        else
                        {
                            if (bdayObj.BirthdayCards == null || bdayObj.BirthdayCards.Count < 100)
                            {
                                UploadPhoto();
                            }
                            else
                            {
                                //message box to buy credits
                                MessageBox.Show(AppResources.CardLimitExceedCaption, AppResources.Only100CardsMessage, MessageBoxButton.OK);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(AppResources.WarnInternetMsg, AppResources.WarnNwTitle, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrAddFriend, MessageBoxButton.OK);
            }
        }

        private void BuyAppMessageBoxDismissed(object sender, DismissedEventArgs e)
        {
            if (!e.Result.Equals(CustomMessageBoxResult.LeftButton)) return;

            var buyAppTask = new MarketplaceDetailTask();
            buyAppTask.Show();
        }

        private void UploadPhoto()
        {
            var photoTask = new PhotoChooserTask
            {
                ShowCamera = true,
                PixelHeight = 400,
                PixelWidth = 400
            };

            photoTask.Completed += PhotoTaskCompleted;
            photoTask.Show();
        }

        private void PhotoTaskCompleted(object sender, PhotoResult e)
        {
            try
            {
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message, AppResources.ErrAddCard, MessageBoxButton.OK);
                }
                else if (e.ChosenPhoto != null)
                {
                    AddCard(e.ChosenPhoto, Path.GetFileName(e.OriginalFileName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrAddCard, MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Add a new birthday card
        /// </summary>
        /// <returns>Guid of the new card</returns>
        private void AddCard(Stream imgStream, string fileName)
        {
            var userId = App.UserPreferences.UserDetails.FacebookId;

            UploadImageToServer(userId, fileName, (FileStream)imgStream);
        }

        private async void UploadImageToServer(String userId, String fileName, FileStream imageStream)
        {
            try
            {
                var service = new AzureStorageService(Services.AzureConnectionString, userId);
                var status = await service.SaveFileToBlob(fileName, imageStream);

                if (status.IsSuccess)
                {
                    var url = Services.CardBaseUrl + userId + "/" + fileName;
                    var newCard = new CardEntity { Url = url };
                    BirthdayUtility.AddBirthdayCard(newCard);

                    var birthdays = DataContext as Birthdays;

                    if (birthdays != null)
                        birthdays.BirthdayCards.Add(newCard);
                }
                else
                {
                    MessageBox.Show(status.ErrorMessage, AppResources.ErrAddCard, MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrAddCard, MessageBoxButton.OK);
            }
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.IsTrial)
                {
                    //messagebox to prompt to buy
                    var buyAppForDeleteMessageBox = new CustomMessageBox
                    {
                        Height = 300,
                        Caption = AppResources.BuyFullVersion,
                        Message = AppResources.BuyAppForDelete,
                        LeftButtonContent = AppResources.BuyLabel,
                        RightButtonContent = AppResources.LaterLabel,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    buyAppForDeleteMessageBox.Dismissed += BuyAppForDeleteMessageBoxDismissed;
                    buyAppForDeleteMessageBox.Show();

                    return;
                    
                }

                var menuItem = sender as MenuItem;

                if (menuItem != null)
                {
                    var birthday = menuItem.DataContext as FriendBirthday;
                    if (birthday != null) BirthdayUtility.DeleteFriend(birthday.Id);
                }

                BindDataContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrDeleting, MessageBoxButton.OK);
            }
        }

        private void BindDataContext()
        {
            var birthdays = BirthdayUtility.GetBirthdays();
            LocalizeBirthdayText(birthdays);

            birthdays.AlphaGroupAllBirthdays = AlphaKeyGroup<FriendBirthday>.CreateGroups
                (
                    birthdays.AllBirthdays,
                    Thread.CurrentThread.CurrentUICulture,
                    f => f.Name, true
                );

            BirthdayCardList.SelectedItems.Clear();
            birthdays.BirthdayCards = new ObservableCollection<CardEntity>();
            DataContext = birthdays;

            foreach (var card in BirthdayUtility.GetBirthdayCards())
            {
                birthdays.BirthdayCards.Add(card);
            }
        }

        /// <summary>
        /// Removes English text with appropriate local language from Birthday
        /// </summary>
        /// <param name="birthdays">The list of recent and all birthdays</param>
        private static void LocalizeBirthdayText(Birthdays birthdays)
        {
            foreach (var birthday in birthdays.AllBirthdays)
            {
                birthday.BirthdayText = birthday.BirthdayText.Replace(Labels.BirthdayLabel, AppResources.BirthdayLabel);
                birthday.BirthdayText = birthday.BirthdayText.Replace(Labels.NotKnownLabel, AppResources.NotKnownLabel);
                birthday.BirthdayText = birthday.BirthdayText.Replace(Labels.TodayLabel, AppResources.TodayLabel);
                birthday.BirthdayText = birthday.BirthdayText.Replace(Labels.TomorrowLabel, AppResources.TomorrowLabel);
                birthday.BirthdayText = birthday.BirthdayText.Replace(Labels.LaterThisWkLabel, AppResources.LaterThisWkLabel);
            }

            foreach (var birthday in birthdays.RecentBirthdays)
            {
                birthday.TimeToEventText = birthday.TimeToEventText.Replace(Labels.BirthdayLabel, AppResources.BirthdayLabel);
                birthday.TimeToEventText = birthday.TimeToEventText.Replace(Labels.NotKnownLabel, AppResources.NotKnownLabel);
                birthday.TimeToEventText = birthday.TimeToEventText.Replace(Labels.TodayLabel, AppResources.TodayLabel);
                birthday.TimeToEventText = birthday.TimeToEventText.Replace(Labels.TomorrowLabel, AppResources.TomorrowLabel);
                birthday.TimeToEventText = birthday.TimeToEventText.Replace(Labels.LaterThisWkLabel, AppResources.LaterThisWkLabel);
                birthday.TimeToEventText = birthday.TimeToEventText.Replace(Labels.DaysLabel, AppResources.DaysLabel);
            }
        }

        private void SetAsDefault(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = sender as MenuItem;

                if (menuItem != null)
                {
                    var card = menuItem.DataContext as CardEntity;
                    if (card != null) App.UserPreferences.BirthdayWish.CustomPicUrl = card.Url;
                }
                SettingsUtility.UpdateUserSettings(App.UserPreferences);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSaveSettings, MessageBoxButton.OK);
            }
        }
    }
}