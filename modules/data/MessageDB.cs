using Microsoft.EntityFrameworkCore;

namespace self_bot.modules.data
{
    public class MessageDB : DbContext
    {
        public DbSet<Messages> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ServersData.db");
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
    }

    public class Messages
    {
        public ulong Id { get; set; }
        public string Message { get; set; }

        public string Author { get; set; }

        public string MessageType { get; set; }
    }
}