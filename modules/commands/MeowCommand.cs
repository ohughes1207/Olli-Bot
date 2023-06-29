
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using self_bot.modules.personality;

namespace self_bot.modules.commands
{
    internal class MeowCommand : ApplicationCommandModule
    {
        private static DiscordClient _clientInstance => Bot.Client;
        private DiscordUser _user;
        private ulong BotID = 1118358168708329543;


        private static bool IsOn = false;

        [SlashCommand("kittenator", "Enable the kittenator to be targeted at a user >:3")]
        [SlashRequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Toggle(InteractionContext ctx, [Option("user", "Specified user")] DiscordUser user)
        {
            try
            {
                if (user.Id == BotID)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("YOU THINK YOU CAN KITTENATOR THE KITTEN!?\n-----FOOL DETECTED-----"));
                    return;
                }
                if (user.Id == 119904333750861824)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("YOU THINK I WILL TURN ON THE MAKER!?\n-----FOOL DETECTED-----"));
                    return;
                }
                if (IsOn==true)
                {
                    IsOn=!IsOn;
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Kittenator shutting down"));
                    _clientInstance.MessageCreated -= TestMessage;
                    return;
                }
                Console.WriteLine(user.Id);
                IsOn=!IsOn;
                _user = user;

                if (IsOn==true)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("-----BEEP-----\n-----BOOP-----\nKITTENATOR ONLINE"));
                    _clientInstance.MessageCreated += TestMessage;
                }

                else if (IsOn==false)

                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Kittenator shutting down"));
                    _clientInstance.MessageCreated -= TestMessage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command..\n------- EXCEPTION -------\n {ex.Message}\n------- EXCEPTION -------");
            }

        }
        public async Task TestMessage(DiscordClient client, MessageCreateEventArgs e)
        {
            var meow = await Meows.GetMeow();
            if (e.Message.Author.Id == _user.Id)
            {
                await e.Message.RespondAsync($"{meow}");
            }
        }
        [SlashCommand("purgekittenator", "Purge kittenator spam")]
        public async Task PurgeKittenator(InteractionContext ctx, [Option("amount", "Amount of messages to purge")] long x)
        {
            try
            {
                int amount = (int)x;
                var messageList = await ctx.Channel.GetMessagesAsync(500);
                var filteredMessages =  from m in messageList
                                        where m.Author.Id == BotID && m.Content.Contains()
                                        select m;

                var x_Messages_del = filteredMessages.Take(amount);
                await ctx.Channel.DeleteMessagesAsync(x_Messages_del);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"---PURGE COMPLETE---\n{x_Messages_del.Count()} MESSAGES DELETED\n---PURGE COMPLETE---"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command..\n------- EXCEPTION -------\n {ex.Message}\n------- EXCEPTION -------");
            }
        }
    }
}
