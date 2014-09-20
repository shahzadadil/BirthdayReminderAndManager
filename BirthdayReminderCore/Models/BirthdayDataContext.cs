using System.Data.Linq;

namespace BirthdayReminderCore.Models
{
    public class BirthdayDataContext : DataContext
    {
        public BirthdayDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public Table<FriendEntity> Friends;
        public Table<CardEntity> Cards;
    }
}
