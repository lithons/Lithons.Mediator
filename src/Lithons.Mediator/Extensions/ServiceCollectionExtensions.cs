using Lithons.Mediator.Contracts;
using Lithons.Mediator.Exceptions;
using Lithons.Mediator.Middleware.Command;
using Lithons.Mediator.Middleware.Command.Contracts;
using Lithons.Mediator.Middleware.Notification;
using Lithons.Mediator.Middleware.Notification.Contracts;
using Lithons.Mediator.Middleware.Request;
using Lithons.Mediator.Middleware.Request.Contracts;
using Lithons.Mediator.Options;
using Lithons.Mediator.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatorServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediator(Action<MediatorOptions>? configure = null)
        {
            services.Configure(configure ?? (_ => { }));

            services.TryAddScoped<IMediator, Mediator>();
            services.TryAddScoped<IRequestSender>(sp => sp.GetRequiredService<IMediator>());
            services.TryAddScoped<ICommandSender>(sp => sp.GetRequiredService<IMediator>());
            services.TryAddScoped<INotificationSender>(sp => sp.GetRequiredService<IMediator>());
            services.TryAddSingleton<IRequestPipeline, RequestPipeline>();
            services.TryAddSingleton<ICommandPipeline, CommandPipeline>();
            services.TryAddSingleton<INotificationPipeline, NotificationPipeline>();

            return services;
        }

        public IServiceCollection AddRequestHandler<THandler>()
            where THandler : class, IRequestHandler
        {
            foreach (var handler in typeof(THandler).GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            {
                var requestType = handler.GetGenericArguments()[0];
                if (services.Any(d => d.ServiceType == handler))
                    throw new DuplicateHandlerException(requestType);

                services.AddScoped(handler, sp => sp.GetRequiredService<THandler>());
            }

            services.AddScoped<THandler>();
            services.AddScoped<IRequestHandler>(sp => sp.GetRequiredService<THandler>());

            return services;
        }

        public IServiceCollection AddCommandHandler<THandler>()
            where THandler : class, ICommandHandler
        {
            foreach (var handler in typeof(THandler).GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            {
                var commandType = handler.GetGenericArguments()[0];
                if (services.Any(d => d.ServiceType == handler))
                    throw new DuplicateHandlerException(commandType);

                services.AddScoped(handler, sp => sp.GetRequiredService<THandler>());
            }

            foreach (var handler in typeof(THandler).GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                var commandType = handler.GetGenericArguments()[0];
                if (services.Any(d => d.ServiceType == handler))
                    throw new DuplicateHandlerException(commandType);

                services.AddScoped(handler, sp => sp.GetRequiredService<THandler>());
            }

            services.AddScoped<THandler>();
            services.AddScoped<ICommandHandler>(sp => sp.GetRequiredService<THandler>());

            return services;
        }

        public IServiceCollection AddNotificationHandler<THandler>()
            where THandler : class, INotificationHandler
        {
            services.AddScoped<THandler>();
            services.AddScoped<INotificationHandler>(sp => sp.GetRequiredService<THandler>());

            foreach (var handler in typeof(THandler).GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
            {
                services.AddScoped(handler, sp => sp.GetRequiredService<THandler>());
            }

            return services;
        }
    }
}
