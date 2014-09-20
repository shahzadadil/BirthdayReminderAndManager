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
        public static UserSettings getUserPreferences()
        {
            IsolatedStorageFileStream fileStream = null;
            try
            {
                IsolatedStorageFile fileLocation = IsolatedStorageFile.GetUserStoreForApplication();
                if (fileLocation.FileExists(FileSystem.SettingsFile))
                {
                    fileStream = new IsolatedStorageFileStream(FileSystem.SettingsFile, FileMode.Open, fileLocation);
                    XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
                    UserSettings userSettings = (UserSettings)serializer.Deserialize(fileStream);
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
                    UserSettings settings = new UserSettings();
                    updateUserSettings(settings);
                    return settings;
                }
            }
            catch (Exception)
            {
                throw;
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
        public static void updateUserSettings(UserSettings settings)
        {
            IsolatedStorageFileStream fileStream=null;
            try
            {
                fileStream = new IsolatedStorageFileStream(FileSystem.SettingsFile, FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication());
                XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
                serializer.Serialize(fileStream, settings);
            }
            catch (Exception)
            {
                throw;
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
