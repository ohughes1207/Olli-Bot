using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OlliBot.Application;
public static class DependencyInjection
{
    [Obsolete("Obsolete since implementing MediatR")]
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration cfg)
    {

        //services.AddTransient<AddMessageHandler>();
        //services.AddTransient<CallMessageHandler>();
        //services.AddTransient<DeleteMessageHandler>();
        //services.AddTransient<ListMessageHandler>();
        //services.AddTransient<UpdateMessageHandler>();

        //services.AddTransient<UpdateEmoteRankingHandler>();
        //services.AddTransient<ClearEmoteRankingHandler>();

        //services.AddTransient<GetAllHumbleBundlesHandler>();
        //services.AddTransient<AddHumbleBundleSubscriberHandler>();
        //services.AddTransient<CheckForHumbleBundleUpdatesHandler>();

        return services;
    }
}
