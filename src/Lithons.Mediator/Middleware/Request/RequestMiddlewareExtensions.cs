using Lithons.Mediator.Abstractions.Middleware.Request;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.Middleware.Request;

/// <summary>
/// Extension methods for adding middleware components to an <see cref="IRequestPipelineBuilder"/>.
/// </summary>
public static class RequestMiddlewareExtensions
{
    extension(IRequestPipelineBuilder builder)
    {
        /// <summary>
        /// Adds a strongly typed middleware component to the request pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The <see cref="IRequestMiddleware"/> type to add.</typeparam>
        /// <param name="args">Additional constructor arguments for the middleware instance.</param>
        /// <returns>The <see cref="IRequestPipelineBuilder"/> for chaining.</returns>
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

        /// <summary>
        /// Adds the built-in request handler invoker to the pipeline.
        /// </summary>
        /// <returns>The <see cref="IRequestPipelineBuilder"/> for chaining.</returns>
        public IRequestPipelineBuilder UseRequestHandlers()
        {
            return builder.UseMiddleware<Components.RequestHandlerInvokerMiddleware>();
        }
    }
}
