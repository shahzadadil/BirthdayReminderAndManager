using System;

namespace BirthdayReminderCore.Models
{
    public static class Constants
    {
        public static readonly String AppId = "176753902512301";
        public static readonly String StoreAppId = "1688659f-1fd5-47d2-b98d-4f2fb64758d0";
        public static readonly String UrlParamaterSeparator = "&";
        public static readonly String FriendEmailTemplate = "<tr><td><img src='[[imagesource]]' alt='profile'/></td><td><a href='[[profileurl]]'>[[friendname]]</a></td></tr>";
        public static readonly String ReminderContainerTemplate = "<html><head></head><body><div><div>[[heading]]</div><div style='height:20px;'></div><div><div><table>" +
                                                                "[[friendplaceholder]]</table></div></div></div></body></html>";
        public static readonly String DefaultProfilePic = "http://bullard.esc.cam.ac.uk/~deuss/wp-content/uploads/2012/08/unknown-person.jpg";
        public static readonly String ActionDisabledErr = "BNS Error: The action is disabled";
        public static readonly String MaxReachedError = "BNS Error: The maximum number of ScheduledActions of this type have already been added.";
    }

    public static class FileSystem
    {
        public static readonly String FriendInfoDirectory = "FriendsInfo";
        public static readonly String ProfilePictureDirectory = "shared/shellcontent/";
        public static readonly String FriendListFile = "FriendList.xml";
        public static readonly String SettingsFile = "Settings.xml";
        public static readonly String LogFile = "Logs.txt";
        public static readonly String ResourceFile = "Resources.xml";
        public static readonly String ProfileImageFolderPath = "/Assets/ProfileImages/";
        public static readonly String BirthdayCardFile = "BirthdayCards.xml";
        public static readonly String FtpImageFolder = "BirthdayReminder/Cards";
        public static readonly String FtpUserName = "winappsftp";
        public static readonly String FtpPassword = "msa07031988";
    }

    public static class Database
    {
        public static readonly String DbConnectionString = "Data Source=isostore:/ToDo.sdf";
    }

    public static class Services
    {
        public static readonly String AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=birthdayremindercards;"
                                            + "AccountKey=u1wQasN9FtcoHDJn747G/oUtWccJfUdWVkgmsMlpVMhRPyLdFNKyyqz4+GP/4g/S+SzvobpDhhr8tCyWhNo0oQ==";

        public static readonly String CardBaseUrl = "http://birthdayremindercards.blob.core.windows.net/";
    }

    public static class BirthdayAppInfo
    {
        public static readonly String BackgroundAgentName = "BirthdayReminderAgent";
    }

    public static class UriParameter
    {
        public static readonly String FeedDialogUrl = "https://www.facebook.com/dialog/feed";
        public static readonly String FriendGuid = "Guid";
        public static readonly String FriendId = "FriendId";
        public static readonly String FacebookAppId = "app_id=";
        public static readonly String FeedDisplayType = "display=";
        public static readonly String FeedCaption = "caption=";
        public static readonly String FeedLinkToShare = "link=";
        public static readonly String FeedRedirectUri = "redirect_uri=";
        public static readonly String FeedTo = "to=";
        public static readonly String FeedPicture = "picture=";
        public static readonly String FeedDescription = "description=";
        public static readonly String Action = "Action";
        public static readonly String ReferrerPage = "referrer";
        public static readonly String BirthdaycardBaseUrl = "http://winapps.s3ts.co.in/BirthdayReminder/Cards/";
        public static readonly String IsSyncScneario = "IsSyncScenario";
    }

    public static class Labels
    {
        public static readonly String BirthdayLabel = "Birthday";
        public static readonly String NotKnownLabel = "Not Known";
        public static readonly String TodayLabel = "Today";
        public static readonly String TomorrowLabel = "Tomorrow";
        public static readonly String LaterThisWkLabel = "Later this week";
        public static readonly String DaysLabel = "Days";
    }
}
