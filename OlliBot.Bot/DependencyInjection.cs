using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OlliBot.Application.EmoteRanking.Scanning;
using OlliBot.Application.HumbleBundle;
using OlliBot.Bot.Mappers;
using OlliBot.Bot.Modules.EmoteRanking;
using OlliBot.Bot.Modules.HumbleBundle;
using OlliBot.Bot.Services;
using Quartz;
using Quartz.Util;
using Serilog;

namespace OlliBot.Bot;
internal static class DependencyInjection
{
    private const string HumbleBundleUpdateGroupName = "humble-bundle";
    private const string EmoteRankingGroupName = "emote-ranking";

    internal static IServiceCollection AddBotServices(this IServiceCollection services)
    {
        services.AddTransient<AddMessageCommandMapper>();
        services.AddHostedService<BotHostedService>();

        services.AddSingleton<DiscordEventListener>();

        services.AddTransient<IEmoteCountScanner, EmoteCountScanner>();
        services.AddTransient<IDiscordSubscriberValidater, DiscordSubscriberValidater>();
        services.AddSingleton<IKeyedSemaphore<(ulong, string)>, KeyedSemaphore<(ulong, string)>>();

        return services;
    }

    internal static IServiceCollection AddDiscordServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddSingleton<DiscordLogService>();

        services.AddSingleton<DiscordSocketClient>((serviceProvider) =>
        {
            try
            {
                var discordClient = new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 5000,
                    AlwaysDownloadUsers = true,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.GuildMembers,
                    //GatewayIntents = GatewayIntents.All
                });

                return discordClient;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize Discord Socket Client");
                throw;
            }
        });

        services.AddSingleton((serviceProvider) =>
        {
            DiscordSocketClient discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

            var interaction = new InteractionService(discordClient.Rest, new InteractionServiceConfig
            {
                UseCompiledLambda = true,
                DefaultRunMode = RunMode.Async,
            });

            return interaction;
        });

        services.AddSingleton<IDiscordClient>(sp => sp.GetRequiredService<DiscordSocketClient>());

        return services;
    }

    internal static IServiceCollection AddScheduling(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        // Explicit registration validates its dependencies at startup.
        services.AddScoped<HumbleBundleUpdateJob>();
        services.AddScoped<EmoteRankingUpdateJob>();
        services.AddScoped<EmoteRankingClearJob>();

        services.AddQuartz(quartz =>
        {
            quartz.SchedulerName = "OlliBot";
            quartz.UseInMemoryStore();

            ScheduleHumbleBundleUpdateJob(quartz, configuration);
            ScheduleEmoteRankingUpdateJob(quartz, configuration);
            ScheduleEmoteRankingClearJob(quartz, configuration);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });


        return services;
    }

    private static void ScheduleHumbleBundleUpdateJob(IServiceCollectionQuartzConfigurator quartz, IConfiguration configuration)
    {
        string cronExpression =
            configuration["Scheduling:HumbleBundleUpdate:Cron"]
    ?? throw new InvalidOperationException(
        "The Humble Bundle cron schedule was not configured.");

        if (!CronExpression.IsValidExpression(cronExpression))
        {
            throw new InvalidOperationException(
                $"'{cronExpression}' is not a valid Quartz cron expression.");
        }

        string timeZoneId =
            configuration["Scheduling:HumbleBundleUpdate:TimeZone"]
            ?? "Europe/London";

        TimeZoneInfo timeZone =
            TimeZoneUtil.FindTimeZoneById(timeZoneId);

        var jobKey = new JobKey(
            "check-for-humble-bundle-updates",
            HumbleBundleUpdateGroupName);

        quartz.AddJob<HumbleBundleUpdateJob>(job =>
            job
                .WithIdentity(jobKey)
                .WithDescription(
                    "Checks for new Humble Bundles."));

        quartz.AddTrigger(trigger =>
            trigger
                .WithIdentity(
                    "daily-humble-bundle-update",
                    HumbleBundleUpdateGroupName)
                .ForJob(jobKey)
                .WithCronSchedule(
                    cronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed()));
    }

    private static void ScheduleEmoteRankingUpdateJob(IServiceCollectionQuartzConfigurator quartz, IConfiguration configuration)
    {
        string cronExpression =
            configuration["Scheduling:EmoteRankingUpdate:Cron"]
    ?? throw new InvalidOperationException(
        "The Emote Ranking Update cron schedule was not configured.");

        if (!CronExpression.IsValidExpression(cronExpression))
        {
            throw new InvalidOperationException(
                $"'{cronExpression}' is not a valid Quartz cron expression.");
        }

        string timeZoneId =
            configuration["Scheduling:EmoteRankingUpdate:TimeZone"]
            ?? "Europe/London";

        TimeZoneInfo timeZone =
            TimeZoneUtil.FindTimeZoneById(timeZoneId);

        var jobKey = new JobKey(
            "emote-ranking-update",
            EmoteRankingGroupName);

        quartz.AddJob<EmoteRankingUpdateJob>(job =>
            job
                .WithIdentity(jobKey)
                .WithDescription(
                    "Updates emote rankings for all guilds the bot is a member of."));
        quartz.AddTrigger(trigger =>
            trigger
                .WithIdentity(
                    "emote-ranking-update",
                    EmoteRankingGroupName)
                .ForJob(jobKey)
                .WithCronSchedule(
                    cronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed()));
    }

    private static void ScheduleEmoteRankingClearJob(IServiceCollectionQuartzConfigurator quartz, IConfiguration configuration)
    {
        string cronExpression =
            configuration["Scheduling:EmoteRankingClear:Cron"]
    ?? throw new InvalidOperationException(
        "The Emote Ranking Clear cron schedule was not configured.");

        if (!CronExpression.IsValidExpression(cronExpression))
        {
            throw new InvalidOperationException(
                $"'{cronExpression}' is not a valid Quartz cron expression.");
        }

        string timeZoneId =
            configuration["Scheduling:EmoteRankingClear:TimeZone"]
            ?? "Europe/London";

        TimeZoneInfo timeZone =
            TimeZoneUtil.FindTimeZoneById(timeZoneId);

        var jobKey = new JobKey(
            "emote-ranking-clear",
            EmoteRankingGroupName);

        quartz.AddJob<EmoteRankingClearJob>(job =>
            job
                .WithIdentity(jobKey)
                .WithDescription(
                    "Clears emote rankings for all guilds the bot is a member of."));
        quartz.AddTrigger(trigger =>
            trigger
                .WithIdentity(
                    "emote-ranking-clear",
                    EmoteRankingGroupName)
                .ForJob(jobKey)
                .WithCronSchedule(
                    cronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed()));
    }
    internal static IServiceCollection ConfigureWindowsService(this IServiceCollection services)
    {
        services.AddWindowsService(options =>
        {
            options.ServiceName = "OlliBot";
        });

        return services;
    }
}
