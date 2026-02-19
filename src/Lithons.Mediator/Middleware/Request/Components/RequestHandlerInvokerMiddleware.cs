using Lithons.Mediator.Contracts;
using Lithons.Mediator.Internal;
using Lithons.Mediator.Middleware.Request.Contracts;

namespace Lithons.Mediator.Middleware.Request.Components;

public class RequestHandlerInvokerMiddleware(RequestMiddlewareDelegate next) : IRequestMiddleware
{
    public async ValueTask InvokeAsync(RequestContext context)
    {
        var request = context.Request;
        var requestType = request.GetType();
        var responseType = context.ResponseType;
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handler = HandlerResolver.ResolveSingle(context.ServiceProvider, handlerType, requestType);

        var invoker = HandlerInvokerCache.GetRequestInvoker(requestType, responseType);
        var task = invoker(handler, request, context.CancellationToken);
        await task.ConfigureAwait(false);

        context.Response = HandlerInvokerCache.GetResultExtractor(responseType)(task)!;

        await next(context).ConfigureAwait(false);
    }
}
