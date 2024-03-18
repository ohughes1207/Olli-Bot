using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
        public int ID { get; set; }
        public ulong DiscordMessageID { get; set; }
        public ulong ServerID { get; set; }
        public string? Title {get; set; }

        public string? Content { get; set; }
        public string? Attachments { get; set; }
        [NotMapped]
        public List<string> AttachmentUrls
        {
            get => Attachments == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(Attachments) ?? new List<string>();
            set => Attachments = JsonSerializer.Serialize(value);
        }
        public required string Author { get; set; }

        public ulong AuthorID { get; set; }

        public ulong MessageOriginID { get; set; }

        public required string MessageType { get; set; }
        public required DateTime DateTimeAdded {get; set; }
    }
}