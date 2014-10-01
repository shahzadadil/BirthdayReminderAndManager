using System;
using System.IO.IsolatedStorage;
using BirthdayReminderCore.Models;

namespace BirthdayReminderCore.Utilities
{
    public static class AppUtility
    {
        /// <summary>
        /// Creates db for storage if it does not exist
        /// </summary>
        /// <returns>Boolean value indicating if the db has already been upgraded</returns>
        public static bool UpdateSchema()
        {
            using (var context = new BirthdayDataContext(Database.DbConnectionString))
            {
                if (context.DatabaseExists())
                {
                    return true;
                }
                
                context.CreateDatabase();

                return false;
            }
        }

        public static void MigrateXmlDataToDatabase()
        {
            var friendList = BirthdayUtility.GetFriendBirthdayList();

            if (friendList != null && friendList.Count > 0)
            {
                using (var context = new BirthdayDataContext(Database.DbConnectionString))
                {
                    foreach (var friend in friendList)
                    {
                        var dbFriend = new FriendEntity
                        {
                            BigProfilePictureUrl = friend.BigProfilePictureUrl,
                            Birthday =
                                string.IsNullOrEmpty(friend.Birthday)
                                    ? new DateTime?()
                                    : Convert.ToDateTime(friend.Birthday),
                            Email = friend.Email,
                            FacebookId = friend.Email,
                            IsReminderCreated = false,
                            IsHidden = friend.isHidden,
                            LastToastRaisedYear = friend.LastToastRaisedYear,
                            Name = friend.Name,
                            PhoneNumber = friend.PhoneNumber,
                            ProfilePictureLocation = friend.ProfilePictureLocation,
                            ProfilePictureUrl = friend.ProfilePictureUrl,
                            SendAutoEmailOnBirthday = friend.SendAutoEmailOnBirthday,
                            TypeOfContact = friend.TypeOfContact
                        };

                        context.Friends.InsertOnSubmit(dbFriend);
                    }

                    context.SubmitChanges();
                }
            }

            CleanUpOldFiles();
        }

        private static void CleanUpOldFiles()
        {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            if (storage.FileExists(FileSystem.FriendInfoDirectory + "//" + FileSystem.BirthdayCardFile))
            {
                storage.DeleteFile(FileSystem.FriendInfoDirectory + "//" + FileSystem.BirthdayCardFile);
            }

            if (storage.FileExists(FileSystem.FriendInfoDirectory + "//" + FileSystem.FriendListFile))
            {
                storage.DeleteFile(FileSystem.FriendInfoDirectory + "//" + FileSystem.FriendListFile);
            }
        }
    }
}
