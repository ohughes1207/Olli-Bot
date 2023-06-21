

namespace self_bot.modules.personality
{
    public class Meows
    {
        public static async Task<string> GetMeow()
        {
            Random rnd = new Random();
            string[] meowArray = new string[]
            {
                "Meow :3",
                "Nya :3",
                "*nuzzles*",
                "*purrs*",
            };
            int pull = rnd.Next(meowArray.Length);
            string meow = meowArray[pull].ToString();
            return meow;
        }
    }
}