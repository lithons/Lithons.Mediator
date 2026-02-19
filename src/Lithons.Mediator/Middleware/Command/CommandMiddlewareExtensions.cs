using Lithons.Mediator.Middleware.Command.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Middleware.Command;

public static class CommandMiddlewareExtensions
{
    extension(ICommandPipelineBuilder builder)
    {
        public ICommandPipelineBuilder UseMiddleware<TMiddleware>(params object[] args) where TMiddleware : ICommandMiddleware
        {
            var middlewareType = typeof(TMiddleware);

            return builder.Use(next =>
            {
                var ctorParams = new object[] { next }.Concat(args).ToArray();
                var instance = ActivatorUtilities.CreateInstance(
                    builder.ApplicationServices, middlewareType, ctorParams);

                var invokeMethod = middlewareType.GetMethod("InvokeAsync")
                                   ?? throw new InvalidOperationException(
                                       $"No InvokeAsync method found on {middlewareType.Name}");

                return (CommandMiddlewareDelegate)invokeMethod
                    .CreateDelegate(typeof(CommandMiddlewareDelegate), instance);
            });
        }

        public ICommandPipelineBuilder UseCommandHandlers()
        {
            return builder.UseMiddleware<Components.CommandHandlerInvokerMiddleware>();
        }
    }
}
