using System;
using System.Data.Linq.Mapping;

namespace BirthdayReminderCore.Models
{
    [Table]
    public class CardEntity
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL IDENTITY", AutoSync = AutoSync.OnInsert)]
        public int Id { get; set; }

        [Column]
        public String Url { get; set; }
    }
}
