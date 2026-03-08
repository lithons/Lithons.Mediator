using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Lithons.Mediator.Exceptions;
using Lithons.Mediator.Middleware.Command;
using Lithons.Mediator.Middleware.Notification;
using Lithons.Mediator.Middleware.Request;
using Lithons.Mediator.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Lithons.Mediator.Extensions;

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

            services.TryAddScoped<IMediator, Services.Mediator>();
            services.TryAddScoped<IRequestSender>(sp => sp.GetRequiredService<IMediator>());
            services.TryAddScoped<ICommandSender>(sp => sp.GetRequiredService<IMediator>());
            services.TryAddScoped<INotificationSender>(sp => sp.GetRequiredService<IMediator>());
            services.TryAddSingleton<IRequestPipeline, RequestPipeline>();
            services.TryAddSingleton<ICommandPipeline, CommandPipeline>();
            services.TryAddSingleton<INotificationPipeline, NotificationPipeline>();

            return services;
        }

        internal IServiceCollection AddRequestHandler<THandler>()
            where THandler : class, IRequestHandler
            => services.AddRequestHandler(typeof(THandler));

        internal IServiceCollection AddRequestHandler(Type handlerType)
        {
            if (handlerType.IsGenericTypeDefinition)
            {
                ValidateOpenGenericHandler(handlerType, typeof(IRequestHandler<,>));
                services.AddScoped(typeof(IRequestHandler<,>), handlerType);
            }
            else if (typeof(IRequestHandler).IsAssignableFrom(handlerType))
            {
                RegisterRequestHandlerCore(services, handlerType);
            }
            else
            {
                throw new ArgumentException(
                    $"Type '{handlerType.Name}' must implement IRequestHandler.",
                    nameof(handlerType));
            }

            return services;
        }

        internal IServiceCollection AddCommandHandler<THandler>()
            where THandler : class, ICommandHandler
            => services.AddCommandHandler(typeof(THandler));

        internal IServiceCollection AddCommandHandler(Type handlerType)
        {
            if (handlerType.IsGenericTypeDefinition)
            {
                if (ImplementsOpenGeneric(handlerType, typeof(ICommandHandler<,>)))
                    services.AddScoped(typeof(ICommandHandler<,>), handlerType);
                else if (ImplementsOpenGeneric(handlerType, typeof(ICommandHandler<>)))
                    services.AddScoped(typeof(ICommandHandler<>), handlerType);
                else
                    throw new ArgumentException(
                        $"Type '{handlerType.Name}' must implement ICommandHandler<> or ICommandHandler<,>.",
                        nameof(handlerType));
            }
            else if (typeof(ICommandHandler).IsAssignableFrom(handlerType))
            {
                RegisterCommandHandlerCore(services, handlerType);
            }
            else
            {
                throw new ArgumentException(
                    $"Type '{handlerType.Name}' must implement ICommandHandler.",
                    nameof(handlerType));
            }

            return services;
        }

        internal IServiceCollection AddNotificationHandler<THandler>()
            where THandler : class, INotificationHandler
            => services.AddNotificationHandler(typeof(THandler));

        internal IServiceCollection AddNotificationHandler(Type handlerType)
        {
            if (handlerType.IsGenericTypeDefinition)
            {
                ValidateOpenGenericHandler(handlerType, typeof(INotificationHandler<>));
                services.AddScoped(typeof(INotificationHandler<>), handlerType);
            }
            else if (typeof(INotificationHandler).IsAssignableFrom(handlerType))
            {
                RegisterNotificationHandlerCore(services, handlerType);
            }
            else
            {
                throw new ArgumentException(
                    $"Type '{handlerType.Name}' must implement INotificationHandler.",
                    nameof(handlerType));
            }

            return services;
        }

        internal IServiceCollection AddHandlersFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)
        {
            var allTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false }
                    && (filter == null || filter(t)));

            foreach (var type in allTypes)
            {
                if (type.IsGenericTypeDefinition)
                {
                    RegisterOpenGenericIfApplicable(services, type);
                    continue;
                }

                if (typeof(IRequestHandler).IsAssignableFrom(type))
                    RegisterRequestHandlerCore(services, type);

                if (typeof(ICommandHandler).IsAssignableFrom(type))
                    RegisterCommandHandlerCore(services, type);

                if (typeof(INotificationHandler).IsAssignableFrom(type))
                    RegisterNotificationHandlerCore(services, type);

                if (typeof(IExceptionHandler).IsAssignableFrom(type))
                    services.TryAddScoped(typeof(IExceptionHandler), type);

                RegisterTypedExceptionHandlerInterfaces(services, type);
            }

            return services;
        }

        internal IServiceCollection AddHandlersFromAssemblyContaining<T>(Func<Type, bool>? filter = null)
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

    private static void ValidateOpenGenericHandler(Type openGenericHandlerType, Type expectedInterface)
    {
        if (!openGenericHandlerType.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"Type '{openGenericHandlerType.Name}' must be an open generic type definition.",
                nameof(openGenericHandlerType));

        if (!ImplementsOpenGeneric(openGenericHandlerType, expectedInterface))
            throw new ArgumentException(
                $"Type '{openGenericHandlerType.Name}' must implement {expectedInterface.Name}.",
                nameof(openGenericHandlerType));
    }

    private static void RegisterTypedExceptionHandlerInterfaces(IServiceCollection services, Type handlerType)
    {
        foreach (var iface in handlerType.GetInterfaces()
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExceptionHandler<>)))
        {
            services.TryAddScoped(iface, handlerType);
        }
    }

    private static bool ImplementsOpenGeneric(Type type, Type openGenericInterface)
        => type.IsGenericTypeDefinition
           && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);

    private static void RegisterOpenGenericIfApplicable(IServiceCollection services, Type type)
    {
        if (ImplementsOpenGeneric(type, typeof(IRequestHandler<,>)))
            services.AddScoped(typeof(IRequestHandler<,>), type);

        if (ImplementsOpenGeneric(type, typeof(ICommandHandler<,>)))
            services.AddScoped(typeof(ICommandHandler<,>), type);
        else if (ImplementsOpenGeneric(type, typeof(ICommandHandler<>)))
            services.AddScoped(typeof(ICommandHandler<>), type);

        if (ImplementsOpenGeneric(type, typeof(INotificationHandler<>)))
            services.AddScoped(typeof(INotificationHandler<>), type);

        if (ImplementsOpenGeneric(type, typeof(IExceptionHandler<>)))
            services.TryAddScoped(typeof(IExceptionHandler<>), type);
    }
}
