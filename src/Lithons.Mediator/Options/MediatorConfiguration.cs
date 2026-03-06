using Lithons.Mediator.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lithons.Mediator.Options;

public sealed class MediatorConfiguration
{
    private readonly IServiceCollection _services;

    internal MediatorConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    public INotificationStrategy DefaultNotificationStrategy { get; set; } = NotificationStrategy.Sequential;
    public ICommandStrategy DefaultCommandStrategy { get; set; } = CommandStrategy.Default;

    public MediatorConfiguration AddHandlersFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)
    {
        _services.AddHandlersFromAssembly(assembly, filter);
        return this;
    }

    public MediatorConfiguration AddHandlersFromAssembly<T>(Func<Type, bool>? filter = null)
    {
        _services.AddHandlersFromAssemblyContaining<T>(filter);
        return this;
    }
}
