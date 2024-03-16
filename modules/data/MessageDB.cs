using Microsoft.EntityFrameworkCore;

namespace self_bot.modules.data
{
    public class MessageDB : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ServersData.db");
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
    }

    public class Message
    {
        public ulong ID { get; set; }
        public ulong DiscordMessageID { get; set; }
        public ulong ServerID { get; set; }
        public string? Title {get; set; }

        public required string Content { get; set; }

        public required string Author { get; set; }

        public ulong AuthorID { get; set; }

        public required string MessageOrigin { get; set; }

        public required string MessageType { get; set; }


    }
}