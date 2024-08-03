using Discord;
using NSubstitute;
using OlliBot.Data;
using OlliBot.Modules;

namespace OlliBot.Tests
{
    public class DatabaseLogicTests
    {

        private readonly IUser _mockUser;
        private readonly IInteractionContext _mockContext;

        public DatabaseLogicTests()
        {
            _mockUser = Substitute.For<IUser>();
            _mockUser.Id.Returns(775196080101619750UL);
            _mockUser.Username.Returns("TestUser");

            _mockContext = Substitute.For<IInteractionContext>();
            _mockContext.Guild.Id.Returns(989440353736214299UL);
            _mockContext.User.Username.Returns("TestUser 2");
            _mockContext.User.Id.Returns(220253209824854016UL);
        }

        //         //
        // Helpers //
        //         //
        public static IEnumerable<object[]> GetTestTypeData()
        {
            yield return new object[] { "", new List<string> { "https://www.youtube.com/" }, "Other" };
            yield return new object[] { ":3", new List<string> { "https://cdn.discordapp.com/attachments/529752744559640597/998720258506493982/unknown.png?ex=66aeb138&is=66ad5fb8&hm=1bb299d8fd12edb060f442272cf5325148d0b6fb84190f5675d90c557d4c7a58&" }, "Meme" };
            yield return new object[] { "kek", new List<string> { }, "Quote" };
            yield return new object[] { "", new List<string> { "https://cdn.discordapp.com/attachments/530102241701396481/1265917629735108628/FB_IMG_1721888665423.jpg?ex=66af1eaa&is=66adcd2a&hm=d49a3f0abe0883c65ec1f7bfd78d4a7a3cf9fdf4fc37d172b5c4d34980351007&" }, "Meme" };
        }
        
        private static IMessage CreateMockMessage(ulong id, string? content, List<string> attachments)
        {
            var mockMessage = Substitute.For<IMessage>();
            mockMessage.Id.Returns(id);
            mockMessage.Content.Returns(content);

            var mockAttachments = new List<IAttachment>();
            foreach (var url in attachments)
            {
                var mockAttachment = Substitute.For<IAttachment>();
                mockAttachment.Url.Returns(url);
                mockAttachments.Add(mockAttachment);
            }

            mockMessage.Attachments.Returns(mockAttachments);

            var mockAuthor = Substitute.For<IUser>();

            mockAuthor.Id.Returns(id);
            mockMessage.Author.Returns(mockAuthor);

            return mockMessage;
        }

        public static IEnumerable<object[]> GetMockMessageData()
        {
            yield return new object[] {
                CreateMockMessage(1111, "TEST MESSAGE", new List<string>()),
                null,
                null,
                new Message
                {
                    DiscordMessageId = 1111,
                    GuildId = 989440353736214299UL,
                    Title = null,
                    Content = "TEST MESSAGE",
                    AttachmentUrls = new List<string>(),
                    Author = "TestUser 2",
                    AuthorId = 220253209824854016UL,
                    MessageOriginId = 1111,
                    DateTimeAdded = DateTime.UtcNow,
                    MessageType = "Quote"
                }
            };
        }

        //       //
        // Tests //
        //       //

        [Theory]
        [MemberData(nameof(GetTestTypeData))]
        public void GetMessageType_ShouldReturnExpectedResult(string entryContent, List<string> attachmentList, string expectedType)
        {
            // Arrange
            Message inputMessage = Substitute.For<Message>();

            inputMessage.Content = entryContent;
            inputMessage.AttachmentUrls = attachmentList;

            // Act
            string result = DatabaseLogic.GetMessageType(inputMessage);

            //Assert
            Assert.Equal(expectedType, result);
        }

        [Theory]
        [MemberData(nameof(GetMockMessageData))]
        public void CreateMessageFromInput_FromIMessage_ShouldReturnExpectedResult(IMessage inputMessage, string? Title, string? messageType, Message expectedMessage)
        {            

            // Act
            var result = DatabaseLogic.CreateMessageFromInput(inputMessage, Title, _mockContext, messageType);

            // Assert

            Assert.Equal(expectedMessage.Id, result.Id);
            Assert.Equal(expectedMessage.GuildId, result.GuildId);
            Assert.Equal(expectedMessage.Title, result.Title);
            Assert.Equal(expectedMessage.Content, result.Content);
            Assert.Equal(expectedMessage.AttachmentUrls, result.AttachmentUrls);
            Assert.Equal(expectedMessage.Author, result.Author);
            Assert.Equal(expectedMessage.AuthorId, result.AuthorId);
            Assert.Equal(expectedMessage.MessageOriginId, result.MessageOriginId);
            Assert.Equal(expectedMessage.MessageType, result.MessageType);

            Assert.InRange(result.DateTimeAdded, expectedMessage.DateTimeAdded.AddSeconds(-1), expectedMessage.DateTimeAdded.AddSeconds(1));
        }

        [Theory]
        [InlineData("test message", null, null)]
        [InlineData("test message", "Test title", null)]
        [InlineData("test message", null, "Meme")]
        [InlineData("test message", "Test title", "Other")]
        public void CreateMessageFromInput_StringInputNoURL_ShouldReturnExpectedResult(string entryContent, string? Title, string? messageType)
        {
            // Arrange
            var expectedAuthor = "TestUser 2";
            var expectedAuthorId = 220253209824854016UL;
            var expectedMessageOriginId = 775196080101619750UL;
            var expectedGuildId = 989440353736214299UL;
            var expectedDateTime = DateTime.UtcNow;


            // Act
            Message result = DatabaseLogic.CreateMessageFromInput(entryContent, Title, _mockContext, messageType, _mockUser);

            // Assert
            Assert.Equal(expectedGuildId, result.GuildId);
            Assert.Equal(Title, result.Title);
            Assert.Equal(entryContent, result.Content);
            Assert.Equal(expectedAuthor, result.Author);
            Assert.Equal(expectedAuthorId, result.AuthorId);
            Assert.Equal(expectedMessageOriginId, result.MessageOriginId);
            if (messageType is null)
            {
                Assert.NotEqual(messageType, result.MessageType);
                Assert.Equal("Quote", result.MessageType);
            }
            else
            {
                Assert.Equal(messageType, result.MessageType);
            }
        }
        [Theory]
        [InlineData(@"test message with url https://youtu.be/dQw4w9WgXcQ?si=uqwvdVsBBR51RLQP", null, null, "test message with url")]
        [InlineData(@"https://map.projectzomboid.com/", "Test title", "Other", "")]
        public void CreateMessageFromInput_StringInputWithURL_ShouldReturnExpectedResult(string entryContent, string? Title, string? messageType, string expectedContent)
        {
            // Arrange
            var expectedAuthor = "TestUser 2";
            var expectedAuthorId = 220253209824854016UL;
            var expectedMessageOriginId = 775196080101619750UL;
            var expectedGuildId = 989440353736214299UL;
            var expectedDateTime = DateTime.UtcNow;


            // Act
            Message result = DatabaseLogic.CreateMessageFromInput(entryContent, Title, _mockContext, messageType, _mockUser);

            // Assert
            Assert.Equal(expectedGuildId, result.GuildId);
            Assert.Equal(Title, result.Title);
            Assert.Equal(expectedContent, result.Content);
            Assert.Equal(expectedAuthor, result.Author);
            Assert.Equal(expectedAuthorId, result.AuthorId);
            Assert.Equal(expectedMessageOriginId, result.MessageOriginId);

            Assert.NotEmpty(result.AttachmentUrls);
            if (messageType is null)
            {
                Assert.NotEqual(messageType, result.MessageType);
                Assert.Equal("Other", result.MessageType);
            }
            else
            {
                Assert.Equal(messageType, result.MessageType);
            }
        }
    }
}