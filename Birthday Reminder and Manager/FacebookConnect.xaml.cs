using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Phone.UserData;
using BirthdayReminder.Utilities;
using BirthdayReminder.Models;
using BirthdayReminder.Resources;
using System.Threading.Tasks;


namespace BirthdayReminder
{
    public partial class FacebookConnect
    {
        private List<FriendEntity> _savedFriendList = new List<FriendEntity>();
        private String _isSync = "No";

        public FacebookConnect()
        {
            InitializeComponent();
            Loaded += FacebookConnectLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationService.BackStack.Any())
            {
                if (NavigationService.BackStack.ElementAt(0).Source.ToString().Contains("FacebookLogin.xaml"))
                {
                    StatusText.Text = AppResources.DownloadingFbContact;
                    ContentStackPanel.Opacity = 0.2;
                    StatusPanel.Visibility = Visibility.Visible;
                }
                NavigationService.RemoveBackEntry();
            }

            NavigationContext.QueryString.TryGetValue(UriParameter.IsSyncScneario, out _isSync);
        }

        private async void FacebookConnectLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await SaveFriends();
                await SaveUserFacebookDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSaveBday, MessageBoxButton.OK);
            }
            finally
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private async Task SaveFriends()
        {
            try
            {
                var friendListTask = await DownloadFriends(App.AccessToken);
                friendListTask = App.IsTrial ? friendListTask.Take(100).ToList() : friendListTask;
                _savedFriendList = BirthdayUtility.GetFriendList();

                if (!App.UserPreferences.ContactSync.FacebookSyncEnabled.HasValue || App.UserPreferences.ContactSync.FacebookSyncEnabled.Value)
                {
                    //display status message
                    StatusText.Text = AppResources.RetrievingFbFriend;
                    foreach (var user in friendListTask)
                    {
                        //check if the friend exists already
                        if (_savedFriendList != null && _savedFriendList.Count > 0 
                                && _savedFriendList.Exists(f => !String.IsNullOrEmpty(f.FacebookId) 
                                    && f.FacebookId.Equals(user.FacebookId, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            UpdateExistingFriendToList(user, _savedFriendList.Find(x => !String.IsNullOrEmpty(x.FacebookId) 
                                    && x.FacebookId.Equals(user.FacebookId, StringComparison.InvariantCultureIgnoreCase)));
                        }
                        else
                        {
                            BirthdayUtility.AddNewFriend(user);
                        }
                    }
                }

                //Save friend list to file
                StatusText.Text = AppResources.SaveBdays;

                //update the reminder list
                StatusText.Text = AppResources.CreateBdayReminder;
                ReminderUtility.UpdateCalendarEntries();
                //BirthdayUtility.downloadProfileImages(isSync);

                //update last sync time in settings and app user preferences
                App.UserPreferences.ContactSync.LastSync = DateTime.Now;
                if (App.UserPreferences != null)
                {
                    SettingsUtility.UpdateUserSettings(App.UserPreferences);
                }

                await BirthdayUtility.SyncCards(App.UserPreferences.UserDetails.FacebookId);

                //update card sync status
                App.UserPreferences.ContactSync.CardsSynced = true;
                if (App.UserPreferences != null)
                {
                    SettingsUtility.UpdateUserSettings(App.UserPreferences);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrBuildBdayList, MessageBoxButton.OK);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private static async Task<List<FriendEntity>> DownloadFriends(String accessToken)
        {
            var facebookFriendList = new List<FriendEntity>();
            var client = new Facebook.FacebookClient(accessToken);
            dynamic result = await client.GetTaskAsync("fql", new
            {
                q = "SELECT uid,name,pic_big,birthday_date FROM user WHERE uid in (SELECT uid2 FROM friend where uid1 = me());"
            });
            //JArray resultsList = JObject.Parse(result.ToString())["data"];
            var resultsList = result["data"];
            for (var i = 0; i < resultsList.Count; i++)
            {
                try
                {
                    var friendObject = resultsList[i];

                    var friend = new FriendEntity
                    {
                        FacebookId = friendObject.uid.ToString(),
                        Name = friendObject.name,
                        ProfilePictureUrl = friendObject.pic_big,
                        Birthday =
                            (friendObject.birthday_date == null)
                                ? String.Empty
                                : BirthdayUtility.FormatFBUserBirthday(friendObject.birthday_date.ToString())
                    };

                    facebookFriendList.Add(friend);
                }
                catch (Exception ex)
                {
                    AppLog.WriteToLog(DateTime.Now, "Error getting friend data from object" + ex.Message + ex.StackTrace + ex.InnerException + ex.Data, LogLevel.Error);
                }
            }

            return facebookFriendList;
        }


        /// <summary>
        /// Update the saved friend details with the new details downloaded
        /// </summary>
        /// <param name="downloadedFriend">The newly downloaded friend object</param>
        /// <param name="existingFriend">The existing friend details stored locally</param>
        private static void UpdateExistingFriendToList(FriendEntity downloadedFriend, FriendEntity existingFriend)
        {
            //update the properties of existing friend
            existingFriend.Birthday = downloadedFriend.Birthday;
            existingFriend.Name = downloadedFriend.Name;
            existingFriend.ProfilePictureUrl = downloadedFriend.ProfilePictureUrl;
        }

        void ContactsSearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            try
            {
                //add local contacts to the list
                //if (e.Results != null && App.UserPreferences.ContactSync.LocalSyncEnabled)
                //{
                //    foreach (Contact contact in e.Results)
                //    {
                //        if (contact.Birthdays != null && contact.Birthdays.Count() > 0)
                //        {
                //            DateTime birthday = contact.Birthdays.ElementAt(0);
                //            String email = (contact.EmailAddresses != null && contact.EmailAddresses.Count() > 0) ? contact.EmailAddresses.ElementAt(0).EmailAddress : String.Empty;
                //            String phoneNumber = (contact.PhoneNumbers != null && contact.PhoneNumbers.Count() > 0) ? contact.PhoneNumbers.ElementAt(0).PhoneNumber : String.Empty;

                //            //generate GUID
                //            Guid newGuid = Guid.NewGuid();
                //            while (newGuid.Equals(Guid.Empty) || friendObjectsList.Friends.Any(p => p.UniqueId.Equals(newGuid.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                //            {
                //                newGuid = Guid.NewGuid();
                //            }

                //            friendObjectsList.Friends.Add(new Friend()
                //            {
                //                UniqueId = newGuid.ToString(),
                //                TypeOfContact = ContactType.Local,
                //                Birthday = String.Format("{0}/{1}/{2}", birthday.Day.ToString(), birthday.Month.ToString(), "1900"),
                //                Email = email,
                //                FacebookId = string.Empty,
                //                Name = contact.CompleteName.ToString(),
                //                PhoneNumber = phoneNumber
                //            });
                //        }
                //    }
                //}

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSaveBdayNContact, MessageBoxButton.OK);
            }
            finally
            {               
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private static async Task SaveUserFacebookDetails()
        {
            var client = new Facebook.FacebookClient(App.AccessToken);
            var result = await client.GetTaskAsync("fql", new
            {
                q = "SELECT uid, name, pic_square, contact_email FROM user WHERE uid = '" + App.FacebookId + "'"
            });
            var jsonCollection = new JsonDictionary(result.ToString().Replace("data:", ""));

            if (jsonCollection.ContainsKey("contact_email"))
            {
                App.UserPreferences.UserDetails.Email = jsonCollection["contact_email"];
            }
            if (jsonCollection.ContainsKey("name"))
            {
                App.UserPreferences.UserDetails.Name = jsonCollection["name"];
            }
            if (jsonCollection.ContainsKey("pic_square"))
            {
                App.UserPreferences.UserDetails.ProfilePicUrl = jsonCollection["pic_square"];
            }
            if (jsonCollection.ContainsKey("user_mobile_phone"))
            {
                App.UserPreferences.UserDetails.ContactNumber = jsonCollection["user_mobile_phone"];
            }
            if (jsonCollection.ContainsKey("uid"))
            {
                App.UserPreferences.UserDetails.FacebookId = jsonCollection["uid"];
            }

            SettingsUtility.UpdateUserSettings(App.UserPreferences);
        }
    }
}