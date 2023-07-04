/*
--------------- This is now redundant  ---------------
--------------- Moved this into Bot.cs ---------------

using System.Text.Json.Serialization;

namespace self_bot
{
    public readonly struct Config
    {
        //Rename JsonPropertyName to JsonProperty if Newtonsoft.json is prefered
        [JsonPropertyName("token")]
        public string Token { get; init; }
        [JsonPropertyName("BotChannel")]
        public ulong BotChannel { get; init; }
        [JsonPropertyName("OwnerID")]
        public ulong OwnerID { get; init; }
    }
}
*/