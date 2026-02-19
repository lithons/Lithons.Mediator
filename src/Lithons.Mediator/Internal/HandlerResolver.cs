using Lithons.Mediator.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Internal;

internal static class HandlerResolver
{
    public static object ResolveSingle(IServiceProvider serviceProvider, Type handlerType, Type messageType)
    {
        try
        {
            return serviceProvider.GetRequiredService(handlerType);
        }
        catch (InvalidOperationException ex)
        {
            throw new HandlerNotFoundException(messageType, ex);
        }
    }
}
