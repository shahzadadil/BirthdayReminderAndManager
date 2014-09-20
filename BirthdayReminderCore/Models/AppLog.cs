using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace BirthdayReminderCore.Models
{
    public static class AppLog
    {
        public static void WriteToLog(DateTime timeStamp, String message, LogLevel logLevel)
        {
            IsolatedStorageFile storageLocation = null;
            IsolatedStorageFileStream stream = null;
            try
            {
                storageLocation = IsolatedStorageFile.GetUserStoreForApplication();
                stream = storageLocation.OpenFile(FileSystem.LogFile, FileMode.Append);
                var logText = String.Format("[{0}] : Level - {1}. Message : {2} " + Environment.NewLine + Environment.NewLine, timeStamp, message, logLevel);
                var logBytes = Encoding.UTF8.GetBytes(logText);
                stream.Write(logBytes, 0, logBytes.Length);
            }
            catch (Exception)
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (storageLocation != null)
                {
                    storageLocation.Dispose();
                }
            }
        }

        public static void CleanUpLogFile()
        {
            var fileInfo = new FileInfo(FileSystem.LogFile);

            if (fileInfo.Length <= 1048576) return;

            fileInfo.Delete();
        }
    }

    public enum LogLevel
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }
}
