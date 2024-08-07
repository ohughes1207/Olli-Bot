using Discord;
using NSubstitute;
using OlliBot.Modules;
using OlliBot.Utilities;
using Xunit;

namespace OlliBot.Tests
{
    public class HelperTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("https://youtu.be/dQw4w9WgXcQ?si=uqwvdVsBBR51RLQP", true)]
        [InlineData("https://cdn.discordapp.com", true)]
        [InlineData("LMAOOOOOOOOOOOOO", false)]
        [InlineData(":3", false)]
        [InlineData("https://map.projectzomboid.com/", true)]
        [InlineData("https://mail.google.com/mail/u/0/#inbox", true)]
        [InlineData("https://www.twitch.tv", true)]
        [InlineData("https://dnd5e.wikidot.com", true)]
        [InlineData("Check out my web app: https://olli-pokedexwebapp.vercel.app/", true)]
        public void HasURL_ShouldReturnExpectedResult(string? input, bool expected)
        {

            // Act
            var result = Helpers.HasURL(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}