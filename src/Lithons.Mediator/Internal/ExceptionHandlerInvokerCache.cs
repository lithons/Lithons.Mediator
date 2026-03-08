using Lithons.Mediator.Abstractions.Contracts;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Lithons.Mediator.Internal;

internal static class ExceptionHandlerInvokerCache
{
    private static readonly ConcurrentDictionary<Type, Func<object, Exception, object, CancellationToken, ValueTask<bool>>> _invokers = new();

    public static Func<object, Exception, object, CancellationToken, ValueTask<bool>> GetInvoker(Type messageType)
        => _invokers.GetOrAdd(messageType, static mType =>
        {
            var handlerInterface = typeof(IExceptionHandler<>).MakeGenericType(mType);
            var handleMethod = handlerInterface.GetMethod(nameof(IExceptionHandler<>.Handle))!;

            var handlerParam = Expression.Parameter(typeof(object), "handler");
            var exParam = Expression.Parameter(typeof(Exception), "ex");
            var msgParam = Expression.Parameter(typeof(object), "msg");
            var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

            var call = Expression.Call(
                Expression.Convert(handlerParam, handlerInterface),
                handleMethod,
                exParam,
                Expression.Convert(msgParam, mType),
                ctParam);

            return Expression.Lambda<Func<object, Exception, object, CancellationToken, ValueTask<bool>>>(
                call, handlerParam, exParam, msgParam, ctParam).Compile();
        });
}
