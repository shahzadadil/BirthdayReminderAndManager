using System;
using System.Data.Linq.Mapping;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;
using BirthdayReminder.ViewModels;

namespace BirthdayReminderCore.Models
{
    [Table]
    public class FriendEntity
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL IDENTITY", AutoSync = AutoSync.OnInsert)]
        public int UniqueId { get; set; }

        [Column]
        public ContactType TypeOfContact { get; set; }

        [Column]
        public string FacebookId { get; set; }

        [Column]
        public string Name { get; set; }

        [Column(CanBeNull = true)]
        public DateTime? Birthday { get; set; }

        [Column]
        public string Email { get; set; }

        [Column]
        public string PhoneNumber { get; set; }

        [Column]
        public string ProfilePictureUrl { get; set; }

        [Column]
        public string BigProfilePictureUrl { get; set; }

        [Column]
        public string ProfilePictureLocation { get; set; }

        [Column]
        public bool SendAutoEmailOnBirthday { get; set; }

        [Column]
        public bool IsReminderCreated { get; set; }

        [Column]
        public bool? IsHidden { get; set; }

        [Column]
        public int LastToastRaisedYear { get; set; }

        public FriendEntity()
        {
            SendAutoEmailOnBirthday = true;
            IsReminderCreated = false;
            IsHidden = false;
            LastToastRaisedYear = 1900;
        }

        public async Task DownloadProfilePicture()
        {
            await DownloadFriendImage(ProfilePictureUrl);
        }

        private async Task DownloadFriendImage(String url)
        {
            var client = new WebClient();
            client.OpenReadCompleted += client_OpenReadCompleted;
            client.OpenReadAsync(new Uri(url, UriKind.Absolute));
        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            IsolatedStorageFileStream fileStream = null;
            try
            {
                var fileName = FileSystem.ProfilePictureDirectory + UniqueId + ".jpg";
                var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

                if (myIsolatedStorage.FileExists(fileName))
                    myIsolatedStorage.DeleteFile(fileName);

                fileStream = new IsolatedStorageFileStream(fileName, FileMode.Create, myIsolatedStorage);
                //var writer = new StreamWriter(fileStream);
                while (true)
                {
                    var byteData = e.Result.ReadByte();
                    if (byteData != -1)
                    {
                        fileStream.WriteByte((byte)byteData);
                    }
                    else
                    {
                        break;
                    }
                }
                //writer.Write(e.Result);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                AppLog.WriteToLog(DateTime.Now, "Error downloading image. " + ex.Message, LogLevel.Error);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }
    }
}


