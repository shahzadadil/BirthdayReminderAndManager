using System;
using BirthdayReminder.ViewModels;
using System.Xml.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using BirthdayReminderCore.Models;

namespace BirthdayReminderCore.Utilities
{
    public static class SettingsUtility
    {
        public static UserSettings GetUserPreferences()
        {
            IsolatedStorageFileStream fileStream = null;
            try
            {
                var fileLocation = IsolatedStorageFile.GetUserStoreForApplication();
                if (fileLocation.FileExists(FileSystem.SettingsFile))
                {
                    fileStream = new IsolatedStorageFileStream(FileSystem.SettingsFile, FileMode.Open, fileLocation);
                    var serializer = new XmlSerializer(typeof(UserSettings));
                    var userSettings = (UserSettings)serializer.Deserialize(fileStream);
                    if (userSettings != null)
                    {
                        return userSettings;
                    }
                    else
                    {
                        throw new Exception("Unable lo load user preferences");
                    }
                }
                else
                {                    
                    var settings = new UserSettings();
                    UpdateUserSettings(settings);
                    return settings;
                }
            }
            finally
            {
                if (fileStream!=null)
                {
                    fileStream.Close();
                }                
            }
        }

        /// <summary>
        /// Updates or creates a new settings file
        /// </summary>
        /// <param name="settings">The user settings object</param>
        public static void UpdateUserSettings(UserSettings settings)
        {
            IsolatedStorageFileStream fileStream=null;
            try
            {
                fileStream = new IsolatedStorageFileStream(FileSystem.SettingsFile, FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication());
                var serializer = new XmlSerializer(typeof(UserSettings));
                serializer.Serialize(fileStream, settings);
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
