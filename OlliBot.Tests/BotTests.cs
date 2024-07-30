using OlliBot.Utilities;
using Xunit;

namespace OlliBot.Tests
{
    public class Tests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("https://youtu.be/dQw4w9WgXcQ?si=uqwvdVsBBR51RLQP", true)]
        [InlineData("https://cdn.discordapp.com", true)]
        [InlineData("LMAOOOOOOOOOOOOO", false)]
        [InlineData("ok.bro", false)]
        [InlineData("https://map.projectzomboid.com/", true)]
        [InlineData("https://mail.google.com/mail/u/0/#inbox", true)]
        [InlineData("https://www.twitch.tv", true)]
        [InlineData("https://dnd5e.wikidot.com", true)]
        [InlineData("https://olli-pokedexwebapp.vercel.app/", true)]
        public void HasURL_ShouldReturnExpectedResult(string? input, bool expected)
        {

            // Act
            var result = Helpers.HasURL(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}