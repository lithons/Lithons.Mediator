using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Channels;

namespace Lithons.Mediator.Extensions;

internal static class BackgroundCommandsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddBackgroundCommandProcessing(
            Action<BoundedChannelOptions>? configureChannel = null)
        {
            services.TryAddSingleton<ICommandsChannel>(_ =>
            {
                if (configureChannel is not null)
                {
                    var options = new BoundedChannelOptions(capacity: 1000)
                    {
                        FullMode = BoundedChannelFullMode.Wait,
                        SingleReader = true
                    };
                    configureChannel(options);
                    return new DefaultCommandsChannel(Channel.CreateBounded<CommandContext>(options));
                }

                return new DefaultCommandsChannel(
                    Channel.CreateUnbounded<CommandContext>(
                        new UnboundedChannelOptions { SingleReader = true }));
            });

            services.AddHostedService<CommandsBackgroundService>();
            return services;
        }
    }
}
