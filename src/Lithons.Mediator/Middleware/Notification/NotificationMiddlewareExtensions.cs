using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Middleware.Notification;

public static class NotificationMiddlewareExtensions
{
    extension(INotificationPipelineBuilder builder)
    {
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

        public INotificationPipelineBuilder UseNotificationHandlers()
        {
            return builder.UseMiddleware<Components.NotificationHandlerInvokerMiddleware>();
        }
    }
}
