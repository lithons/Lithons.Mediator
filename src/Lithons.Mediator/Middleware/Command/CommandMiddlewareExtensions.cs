using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Middleware.Command;

/// <summary>
/// Extension methods for adding middleware components to an <see cref="ICommandPipelineBuilder"/>.
/// </summary>
public static class CommandMiddlewareExtensions
{
    extension(ICommandPipelineBuilder builder)
    {
        /// <summary>
        /// Adds a strongly typed middleware component to the command pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The <see cref="ICommandMiddleware"/> type to add.</typeparam>
        /// <param name="args">Additional constructor arguments for the middleware instance.</param>
        /// <returns>The <see cref="ICommandPipelineBuilder"/> for chaining.</returns>
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

        /// <summary>
        /// Adds the built-in command handler invoker to the pipeline.
        /// </summary>
        /// <returns>The <see cref="ICommandPipelineBuilder"/> for chaining.</returns>
        public ICommandPipelineBuilder UseCommandHandlers()
        {
            return builder.UseMiddleware<Components.CommandHandlerInvokerMiddleware>();
        }
    }
}
