using System.Threading.Tasks;
using BirthdayReminder.ViewModels;
using BirthdayReminderCore.Models;
using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Serialization;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace BirthdayReminderCore.Utilities
{
    public static class BirthdayUtility
    {        
        /// <summary>
        /// Loads the birthdays and friend details from file
        /// </summary>
        /// <returns>List of friend details</returns>
        public static List<Friend> GetFriendBirthdayList()
        {
            IsolatedStorageFileStream stream = null;
            try
            {
                var friendList = new FriendList {Friends = new List<Friend>()};
                var fileLocation = IsolatedStorageFile.GetUserStoreForApplication();
                if (fileLocation.FileExists(FileSystem.FriendInfoDirectory + "//" + FileSystem.FriendListFile))
                {
                    stream = new IsolatedStorageFileStream(FileSystem.FriendInfoDirectory + "//" + FileSystem.FriendListFile, FileMode.Open, fileLocation);
                    var serializer = new XmlSerializer(typeof(FriendList));
                    friendList = (FriendList)serializer.Deserialize(stream);
                }
                return friendList.Friends;
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
        /// Get the list of all friends from database
        /// </summary>
        /// <returns>List of friends</returns>
        public static List<FriendEntity> GetFriendList()
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var friendList = (from friend in context.Friends
                    select friend).ToList();

                return friendList;
            }
        }

        /// <summary>
        /// Calculates birthdays and builds a list 
        /// </summary>
        /// <returns>List of Birthdays</returns>
        public static Birthdays GetBirthdays()
        {
            try
            {
                var context = new BirthdayDataContext(Database.DbConnectionString);

                var birthdays = (from friend in context.Friends
                                 where friend.IsHidden.HasValue == false || friend.IsHidden.Value == false
                                select new FriendBirthday
                                {
                                    Birthday = friend.Birthday.HasValue ? friend.Birthday.Value : new DateTime?(),
                                    Id = friend.UniqueId,
                                    Name = friend.Name,
                                    FacebookId = friend.FacebookId,
                                    ProfilePicUrl = friend.ProfilePictureUrl,
                                    DaysAhead = DateTimeUtility.GetTimeToEvent(friend.Birthday),
                                    TimeToEventText = GetRecentBirthdayText(friend.Birthday)
                                })
                                .ToList();

                if (!birthdays.Any())
                {
                    throw new Exception("Friend List is empty. Please setup your friends list.");
                }

                foreach (var birthday in birthdays)
                {
                    birthday.BirthdayText = Labels.BirthdayLabel + " : " +
                                            (birthday.Birthday.HasValue
                                                ? birthday.Birthday.Value.ToString("dd MMM")
                                                : Labels.NotKnownLabel);
                }

                context.Dispose();

                return new Birthdays
                {
                    AllBirthdays = birthdays.OrderBy(p => p.Name).ToList(),
                    RecentBirthdays = birthdays.FindAll(b => !string.IsNullOrEmpty(b.TimeToEventText))
                        .OrderBy(p => p.DaysAhead)
                        .Take(7)
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get the text to be displayed as birthday status
        /// </summary>
        /// <param name="birthday">The date of the birthday</param>
        /// <returns>String representation of the birthday</returns>
        private static string GetRecentBirthdayText(DateTime? birthday)
        {
            var daysAhead = DateTimeUtility.GetTimeToEvent(birthday);
            var birthdayText=string.Empty;

            if (!daysAhead.HasValue)
            {
                return string.Empty;
            }

            switch (daysAhead)
            {
                case 0:
                    birthdayText = Labels.TodayLabel;
                    break;
                case 1:
                    birthdayText = Labels.TomorrowLabel;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    if (birthday != null)
                        birthdayText = Labels.LaterThisWkLabel + " : " + birthday.Value.ToString("dd MMM");
                    break;
                default:
                    birthdayText = daysAhead + " " + Labels.DaysLabel;
                    break;
            }

            return birthdayText;
        }

        /// <summary>
        /// Retrieves the friend details from file using GUID
        /// </summary>
        /// <param name="friendId">GUID of friend</param>
        /// <returns>The Friend details object</returns>
        public static FriendEntity GetFriendDetailsById(int friendId)
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var friendDetails = (from friend in context.Friends
                    where friend.UniqueId == friendId
                    select friend).Single();

                return friendDetails;
            }
        }

        /// <summary>
        /// Updates the details of a particulat friend
        /// </summary>
        /// <param name="friendDetails">The details of the friend to update</param>
        public static void UpdateFriendDetails(FriendEntity friendDetails)
        {  
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var existingFriend = (from friend in context.Friends
                    where friend.UniqueId == friendDetails.UniqueId
                    select friend).FirstOrDefault();

                CopyProperties(existingFriend, friendDetails);
                //considering the scenario where the data should be automatically updated as reference has been updated
                context.SubmitChanges();
            }
        }

        /// <summary>
        /// Update the properties if the freind details
        /// </summary>
        /// <param name="existingFriend">Existing friend details</param>
        /// <param name="updatedFriend">Friend details with updated properties</param>
        private static void CopyProperties(FriendEntity existingFriend, FriendEntity updatedFriend)
        {
            existingFriend.BigProfilePictureUrl = updatedFriend.BigProfilePictureUrl;
            existingFriend.Birthday = updatedFriend.Birthday;
            existingFriend.Email = updatedFriend.Email;
            existingFriend.FacebookId = updatedFriend.FacebookId;
            existingFriend.IsHidden = updatedFriend.IsHidden;
            existingFriend.IsReminderCreated = updatedFriend.IsReminderCreated;
            existingFriend.LastToastRaisedYear = updatedFriend.LastToastRaisedYear;
            existingFriend.Name = updatedFriend.Name;
            existingFriend.PhoneNumber = updatedFriend.PhoneNumber;
            existingFriend.ProfilePictureLocation = updatedFriend.ProfilePictureLocation;
            existingFriend.ProfilePictureUrl = updatedFriend.ProfilePictureUrl;
            existingFriend.SendAutoEmailOnBirthday = updatedFriend.SendAutoEmailOnBirthday;
            existingFriend.TypeOfContact = updatedFriend.TypeOfContact;
            existingFriend.UniqueId = updatedFriend.UniqueId;
        }

        /// <summary>
        /// Add a new friend
        /// </summary>
        /// <param name="friendDetails">The details of the friend to be added</param>
        /// <returns>The Guid of the new friend</returns>
        public static int AddNewFriend(FriendEntity friendDetails)
        {
            using (var context =new BirthdayDataContext(Database.DbConnectionString))
            {
                friendDetails.SendAutoEmailOnBirthday = true;
                friendDetails.IsReminderCreated = false;
                friendDetails.IsHidden = false;
                friendDetails.LastToastRaisedYear = 1900;

                context.Friends.InsertOnSubmit(friendDetails);
                context.SubmitChanges();

                return friendDetails.UniqueId;
            }
        }

        /// <summary>
        /// Hides a friend record from display
        /// </summary>
        /// <param name="friendId">ID of the friend</param>
        public static void DeleteFriend(int friendId)
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var friendToDelete = (from friend in context.Friends
                    where friend.UniqueId == friendId
                    select friend).Single();

                friendToDelete.IsHidden = true;

                context.SubmitChanges();
            }
        }

        /// <summary>
        /// Hides multiple friends from display
        /// </summary>
        /// <param name="friendIdList">List of Ids of friends</param>
        public static void DeleteMultipleFriends(IEnumerable<int> friendIdList)
        {
            if (friendIdList == null) return;

            foreach (var friendId in friendIdList)
            {
                DeleteFriend(friendId);
            }
        }

        /// <summary>
        /// Download profile images of friends
        /// </summary>
        /// <param name="isSync">Is synchronization action getting performed</param>
        public static async void DownloadProfileImages(String isSync)
        {
            try
            {
                //crreate image directory if it does not exist
                var storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!storage.DirectoryExists(FileSystem.ProfilePictureDirectory))
                {
                    storage.CreateDirectory(FileSystem.ProfilePictureDirectory);
                }

                //get list of FB friends for pic download
                var friendList = GetFriendList();
                var isSyncScenario = (!String.IsNullOrEmpty(isSync) && isSync.Equals("Yes", StringComparison.InvariantCultureIgnoreCase));

                if (friendList == null || friendList.Count <= 0) return;

                foreach (var friend in friendList)
                {
                    try
                    {
                        if (!friend.TypeOfContact.Equals(ContactType.Facebook) || string.IsNullOrEmpty(friend.FacebookId) 
                            || (!String.IsNullOrEmpty(friend.ProfilePictureLocation) && !isSyncScenario) || !NetworkInterface.GetIsNetworkAvailable()) 
                            continue;

                        await friend.DownloadProfilePicture();

                        friend.ProfilePictureLocation = @"isostore:/" + FileSystem.ProfilePictureDirectory + friend.UniqueId + ".jpg";
                        UpdateFriendDetails(friend);
                    }
                    catch (Exception ex)
                    {
                        AppLog.WriteToLog(DateTime.Now, "Error downloading one or more photos. " + ex.Message, LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteToLog(DateTime.Now, "Error downloading one or more photos. " + ex.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// Format the facebook birthday to store lcoally
        /// </summary>
        /// <param name="birthday">Birthday in string format</param>
        /// <returns>Birthday to store</returns>
        public static string FormatUserBirthday(string birthday)
        {
            if (string.IsNullOrEmpty(birthday)) return string.Empty;

            var birthdayString = birthday.Split(new[] { '/', '-' });
            var day = Convert.ToInt32(birthdayString[0].Trim());
            var month = Convert.ToInt32(birthdayString[1].Trim());

            return (new DateTime(1900, month, day)).ToString("dd/MM/yyyy");
        }

        public static DateTime? FormatFBUserBirthday(string birthday)
        {
            if (string.IsNullOrEmpty(birthday)) return null;

            var birthdayString = birthday.Split(new[] { '/', '-' });
            var day = Convert.ToInt32(birthdayString[1].Trim());
            var month = Convert.ToInt32(birthdayString[0].Trim());

            return (new DateTime(1900, month, day));
        }

        public static void AddBirthdayCard(CardEntity newCard)
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                context.Cards.InsertOnSubmit(newCard);
                context.SubmitChanges();
            }
        }

        public static void UpdateUrlInCard(int id, string url)
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var existingCard = (from card in context.Cards
                    where card.Id == id
                    select card).Single();

                existingCard.Url = url;

                context.SubmitChanges();
            }
        }

        /// <summary>
        /// Gets all the cards
        /// </summary>
        /// <returns>List of the cards</returns>
        public static List<CardEntity> GetBirthdayCards()
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var cards = (from card in context.Cards
                    select card).ToList();

                return cards;
            }
        }

        /// <summary>
        /// Gets the details of a crd by its id
        /// </summary>
        /// <param name="id">Id of the card</param>
        /// <returns>The card details</returns>
        public static CardEntity GetBirthdayCardById(int id)
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                var cardDetails = (from card in context.Cards
                    where card.Id == id
                    select card).Single();

                return cardDetails;
            }
        }

        /// <summary>
        /// Deletes a specific card
        /// </summary>
        /// <param name="id">Id of the card to be deleted</param>
        public static void DeleteCard(int id)
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                context.Cards.DeleteOnSubmit(GetBirthdayCardById(id));

                context.SubmitChanges();
            }
        }

        public static void DeleteCards(List<int> cardIds)
        {
            using (var context = new  BirthdayDataContext(Database.DbConnectionString))
            {
                var cardsToDelete = (from card in context.Cards
                    where cardIds.Contains(card.Id)
                    select card).AsEnumerable();

                context.Cards.DeleteAllOnSubmit(cardsToDelete);

                context.SubmitChanges();
            }
        }

        /// <summary>
        /// Save the ocal cards to file
        /// </summary>
        public static void SaveLocalCards()
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                if (context.Cards.Any()) return;

                var cards = new List<CardEntity>
                {
                    new CardEntity{Url = "http://www.simmsmanncenter.ucla.edu/wp-content/uploads/30420-birthday-candles1.jpg"},
                    new CardEntity{Url = "http://images.thefuntimesguide.com/wp-content/blogs.dir/46/files/strawberry-birthday-cake-by-chidorian-thumb-280x278-10085.jpg"}
                }
                .AsEnumerable();

                context.Cards.InsertAllOnSubmit(cards);
                context.SubmitChanges();
            }
        }

        /// <summary>
        /// Returns anonymous unique id for the user
        /// </summary>
        /// <returns>Anonymous user id</returns>
        public static String GetUserAnId()
        {
            object userId;

            if (!UserExtendedProperties.TryGetValue("ANID2", out userId))
                throw new Exception("Unable to retrieve user id");

            var anonymousId = userId as String;

            return anonymousId;
        }

        public static async Task SyncCards(string userId)
        {
            var service = new AzureStorageService(Services.AzureConnectionString, userId);
            var onlineCards = (await service.GetBirthdayCards()).ToList();

            var storedCards = GetBirthdayCards();

            var nonExistentCards = onlineCards.Where(c => storedCards.All(sc => sc.Id != c.Id));

            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                context.Cards.InsertAllOnSubmit(nonExistentCards);

                context.SubmitChanges();
            }
        }
    }
}
