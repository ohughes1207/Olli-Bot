using System.Text.Json.Serialization;


namespace OlliBot.Data
{
    public class Config
    {
        //Rename JsonPropertyName to JsonProperty if Newtonsoft.json is prefered
        
        //These properties cannot be modified through commands
        [JsonPropertyName("OwnerID")]
        public ulong OwnerID { get; init; }
        [JsonPropertyName("BotID")]
        public ulong BotID { get; init; }


        //Properties from here onwards can be modified through commands
        [JsonPropertyName("BotChannel")]
        public ulong? BotChannel { get; set; }
    }
}