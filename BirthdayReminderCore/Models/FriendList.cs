﻿
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using BirthdayReminder.ViewModels;
using BirthdayReminderCore.Utilities;
using System.Threading.Tasks;
using BirthdayReminder.Models;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.17929.
// 
namespace BirthdayReminderCore.Models
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]

    [System.Diagnostics.DebuggerStepThroughAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class FriendList
    {
        public List<Friend> Friends { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]

    [System.Diagnostics.DebuggerStepThroughAttribute()]

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class Friend
    {
        public string UniqueId { get; set; }

        public ContactType TypeOfContact { get; set; }
        /// <remarks/>
        public string FacebookId { get; set; }

        /// <remarks/>
        public string Name { get; set; }

        /// <remarks/>
        public string Birthday { get; set; }

        /// <remarks/>
        public string Email { get; set; }

        /// <remarks/>
        public string PhoneNumber { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string ProfilePictureUrl { get; set; }

        public string BigProfilePictureUrl { get; set; }

        /// <remarks/>
        public string ProfilePictureLocation { get; set; }

        public bool SendAutoEmailOnBirthday { get; set; }

        public bool IsReminderCreated { get; set; }

        public bool? isHidden { get; set; }

        public int LastToastRaisedYear { get; set; }

        public Friend()
        {
            SendAutoEmailOnBirthday = true;
            IsReminderCreated = false;
            isHidden = false;
            LastToastRaisedYear = 1900;
        }

        public async Task downloadProfilePicture()
        {
            try
            {
                await this.downloadFriendImage(this.ProfilePictureUrl);
            }
            catch (Exception)
            {
                throw;
            }
        }       

        private async Task downloadFriendImage(String url)
        {
            try
            {
                WebClient client = new WebClient();
                client.OpenReadCompleted += client_OpenReadCompleted;
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            IsolatedStorageFileStream fileStream = null;
            try
            {
                var fileName = FileSystem.ProfilePictureDirectory + this.UniqueId + ".jpg";
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

                if (myIsolatedStorage.FileExists(fileName))
                    myIsolatedStorage.DeleteFile(fileName);

                fileStream = new IsolatedStorageFileStream(fileName, FileMode.Create, myIsolatedStorage);
                //var writer = new StreamWriter(fileStream);
                int byteData = -1;
                while (true)
                {
                    byteData = e.Result.ReadByte();
                    if (byteData == -1)
                    {
                        break;
                    }
                    fileStream.WriteByte((byte)byteData);
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
                if (fileStream!=null)
                {
                    fileStream.Close();
                }
            }
        }        
    }

    public enum ActionType
    {
        Add = 0, Update = 1, Details = 2, Delete = 3
    }
}