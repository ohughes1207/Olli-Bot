using OlliBot.Utilities;
using System.Text.Json.Serialization;


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
        public void HasURL_ShouldReturnExpectedResult(string? input, bool expected)
        {

            // Act
            var result = Helpers.HasURL(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}