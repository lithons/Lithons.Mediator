using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Middleware.Notification;

/// <summary>
/// Extension methods for adding middleware components to an <see cref="INotificationPipelineBuilder"/>.
/// </summary>
public static class NotificationMiddlewareExtensions
{
    extension(INotificationPipelineBuilder builder)
    {
        /// <summary>
        /// Adds a strongly typed middleware component to the notification pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The <see cref="INotificationMiddleware"/> type to add.</typeparam>
        /// <param name="args">Additional constructor arguments for the middleware instance.</param>
        /// <returns>The <see cref="INotificationPipelineBuilder"/> for chaining.</returns>
        public INotificationPipelineBuilder UseMiddleware<TMiddleware>(params object[] args) where TMiddleware : INotificationMiddleware
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

                return (NotificationMiddlewareDelegate)invokeMethod
                    .CreateDelegate(typeof(NotificationMiddlewareDelegate), instance);
            });
        }

        /// <summary>
        /// Adds the built-in notification handler invoker to the pipeline.
        /// </summary>
        /// <returns>The <see cref="INotificationPipelineBuilder"/> for chaining.</returns>
        public INotificationPipelineBuilder UseNotificationHandlers()
        {
            return builder.UseMiddleware<Components.NotificationHandlerInvokerMiddleware>();
        }
    }
}
