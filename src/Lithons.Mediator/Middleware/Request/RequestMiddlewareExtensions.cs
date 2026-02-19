using Lithons.Mediator.Abstractions.Middleware.Request;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Middleware.Request;

public static class RequestMiddlewareExtensions
{
    extension(IRequestPipelineBuilder builder)
    {
        public IRequestPipelineBuilder UseMiddleware<TMiddleware>(params object[] args) where TMiddleware : IRequestMiddleware
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

                return (RequestMiddlewareDelegate)invokeMethod
                    .CreateDelegate(typeof(RequestMiddlewareDelegate), instance);
            });
        }

        public IRequestPipelineBuilder UseRequestHandlers()
        {
            return builder.UseMiddleware<Components.RequestHandlerInvokerMiddleware>();
        }
    }
}
