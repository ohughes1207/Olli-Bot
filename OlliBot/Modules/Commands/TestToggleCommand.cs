/*
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace self_bot.modules.commands
{
    internal class TestToggleCommand : ApplicationCommandModule
    {
        private static DiscordClient _clientInstance => Bot.Client;
        private DiscordUser _user;


        private static bool IsOn = false;

        [SlashCommand("toggletest", "Toggle test messages to be sent at user after every message")]
        public async Task Toggle(InteractionContext ctx, [Option("user", "Specified user")] DiscordUser user)
        {
            try
            {
                Console.WriteLine("1");
                IsOn=!IsOn;
                _user = user;

                if (IsOn==true)
                {
                    Console.WriteLine("2");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Test toggle online"));
                    _clientInstance.MessageCreated += TestMessage;
                }

                if (IsOn==false)

                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Test toggle shutting down"));
                    _clientInstance.MessageCreated += TestMessage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command..\n------- EXCEPTION -------\n {ex.Message}\n------- EXCEPTION -------");
            }

        }
        public async Task TestMessage(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Message.Author.Id == _user.Id)
            {
                await e.Message.RespondAsync("Test success");
            }
        }
    }
}
*/