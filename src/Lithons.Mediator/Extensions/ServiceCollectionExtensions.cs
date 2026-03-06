using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Lithons.Mediator.Exceptions;
using Lithons.Mediator.Middleware.Command;
using Lithons.Mediator.Middleware.Notification;
using Lithons.Mediator.Middleware.Request;
using Lithons.Mediator.Options;
using Lithons.Mediator.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatorServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediator(Action<MediatorConfiguration>? configure = null)
        {
            var config = new MediatorConfiguration(services);
            configure?.Invoke(config);

            services.Configure<MediatorOptions>(opts =>
            {
                opts.DefaultNotificationStrategy = config.DefaultNotificationStrategy;
                opts.DefaultCommandStrategy = config.DefaultCommandStrategy;
            });

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
            RegisterRequestHandlerCore(services, typeof(THandler));
            return services;
        }

        public IServiceCollection AddCommandHandler<THandler>()
            where THandler : class, ICommandHandler
        {
            RegisterCommandHandlerCore(services, typeof(THandler));
            return services;
        }

        public IServiceCollection AddNotificationHandler<THandler>()
            where THandler : class, INotificationHandler
        {
            RegisterNotificationHandlerCore(services, typeof(THandler));
            return services;
        }

        public IServiceCollection AddHandlersFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
                    && (typeof(IRequestHandler).IsAssignableFrom(t)
                        || typeof(ICommandHandler).IsAssignableFrom(t)
                        || typeof(INotificationHandler).IsAssignableFrom(t))
                    && (filter == null || filter(t)));

            foreach (var handlerType in handlerTypes)
            {
                if (typeof(IRequestHandler).IsAssignableFrom(handlerType))
                    RegisterRequestHandlerCore(services, handlerType);

                if (typeof(ICommandHandler).IsAssignableFrom(handlerType))
                    RegisterCommandHandlerCore(services, handlerType);

                if (typeof(INotificationHandler).IsAssignableFrom(handlerType))
                    RegisterNotificationHandlerCore(services, handlerType);
            }

            return services;
        }

        public IServiceCollection AddHandlersFromAssemblyContaining<T>(Func<Type, bool>? filter = null)
            => services.AddHandlersFromAssembly(typeof(T).Assembly, filter);
    }

    private static void RegisterRequestHandlerCore(IServiceCollection services, Type handlerType)
    {
        foreach (var handler in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
        {
            var requestType = handler.GetGenericArguments()[0];
            if (services.Any(d => d.ServiceType == handler))
                throw new DuplicateHandlerException(requestType);

            services.AddScoped(handler, sp => sp.GetRequiredService(handlerType));
        }

        services.AddScoped(handlerType);
        services.AddScoped(typeof(IRequestHandler), sp => sp.GetRequiredService(handlerType));
    }

    private static void RegisterCommandHandlerCore(IServiceCollection services, Type handlerType)
    {
        foreach (var handler in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
        {
            var commandType = handler.GetGenericArguments()[0];
            if (services.Any(d => d.ServiceType == handler))
                throw new DuplicateHandlerException(commandType);

            services.AddScoped(handler, sp => sp.GetRequiredService(handlerType));
        }

        foreach (var handler in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
        {
            var commandType = handler.GetGenericArguments()[0];
            if (services.Any(d => d.ServiceType == handler))
                throw new DuplicateHandlerException(commandType);

            services.AddScoped(handler, sp => sp.GetRequiredService(handlerType));
        }

        services.AddScoped(handlerType);
        services.AddScoped(typeof(ICommandHandler), sp => sp.GetRequiredService(handlerType));
    }

    private static void RegisterNotificationHandlerCore(IServiceCollection services, Type handlerType)
    {
        services.AddScoped(handlerType);
        services.AddScoped(typeof(INotificationHandler), sp => sp.GetRequiredService(handlerType));

        foreach (var handler in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
        {
            services.AddScoped(handler, sp => sp.GetRequiredService(handlerType));
        }
    }
}
