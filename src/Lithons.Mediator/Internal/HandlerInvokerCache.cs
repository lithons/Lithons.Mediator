using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Lithons.Mediator.Internal;

internal static class HandlerInvokerCache
{
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object, CancellationToken, Task>> _requestInvokers = new();
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object, CancellationToken, Task>> _commandInvokers = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object, CancellationToken, Task>> _voidCommandInvokers = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object, CancellationToken, Task>> _notificationInvokers = new();
    private static readonly ConcurrentDictionary<Type, Func<Task, object?>> _resultExtractors = new();
    private static readonly ConcurrentDictionary<Type, Func<ICommandStrategy, CommandStrategyContext, Task>> _strategyExecuteDelegates = new();

    public static Func<object, object, CancellationToken, Task> GetRequestInvoker(Type requestType, Type responseType)
        => _requestInvokers.GetOrAdd((requestType, responseType), static key =>
        {
            var (rqType, rsType) = key;
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(rqType, rsType);
            var method = handlerType.GetMethod(nameof(IRequestHandler<,>.HandleAsync))!;

            var handlerParam = Expression.Parameter(typeof(object), "h");
            var requestParam = Expression.Parameter(typeof(object), "r");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var call = Expression.Call(
                Expression.Convert(handlerParam, handlerType),
                method,
                Expression.Convert(requestParam, rqType),
                cancellationTokenParam);

            return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                Expression.Convert(call, typeof(Task)),
                handlerParam, requestParam, cancellationTokenParam).Compile();
        });

    public static Func<object, object, CancellationToken, Task> GetCommandInvoker(Type commandType, Type resultType)
        => _commandInvokers.GetOrAdd((commandType, resultType), static key =>
        {
            var (cType, rType) = key;
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(cType, rType);
            var method = handlerType.GetMethod(nameof(ICommandHandler<,>.HandleAsync))!;

            var handlerParam = Expression.Parameter(typeof(object), "h");
            var commandParam = Expression.Parameter(typeof(object), "c");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var call = Expression.Call(
                Expression.Convert(handlerParam, handlerType),
                method,
                Expression.Convert(commandParam, cType),
                cancellationTokenParam);

            return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                Expression.Convert(call, typeof(Task)),
                handlerParam, commandParam, cancellationTokenParam).Compile();
        });

    public static Func<object, object, CancellationToken, Task> GetVoidCommandInvoker(Type commandType)
        => _voidCommandInvokers.GetOrAdd(commandType, static cType =>
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(cType);
            var method = handlerType.GetMethod("HandleAsync")!;

            var handlerParam = Expression.Parameter(typeof(object), "h");
            var commandParam = Expression.Parameter(typeof(object), "c");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var call = Expression.Call(
                Expression.Convert(handlerParam, handlerType),
                method,
                Expression.Convert(commandParam, cType),
                cancellationTokenParam);

            return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                call, handlerParam, commandParam, cancellationTokenParam).Compile();
        });

    public static Func<object, object, CancellationToken, Task> GetNotificationInvoker(Type notificationType)
        => _notificationInvokers.GetOrAdd(notificationType, static nType =>
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(nType);
            var method = handlerType.GetMethod(nameof(INotificationHandler<>.HandleAsync))!;

            var handlerParam = Expression.Parameter(typeof(object), "h");
            var notifParam = Expression.Parameter(typeof(object), "n");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var call = Expression.Call(
                Expression.Convert(handlerParam, handlerType),
                method,
                Expression.Convert(notifParam, nType),
                cancellationTokenParam);

            return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                call, handlerParam, notifParam, cancellationTokenParam).Compile();
        });

    public static Func<Task, object?> GetResultExtractor(Type resultType)
        => _resultExtractors.GetOrAdd(resultType, static rType =>
        {
            var taskType = typeof(Task<>).MakeGenericType(rType);
            var resultProp = taskType.GetProperty(nameof(Task<>.Result))!;

            var taskParam = Expression.Parameter(typeof(Task), "t");
            var body = Expression.Convert(
                Expression.Property(Expression.Convert(taskParam, taskType), resultProp),
                typeof(object));

            return Expression.Lambda<Func<Task, object?>>(body, taskParam).Compile();
        });

    public static Func<ICommandStrategy, CommandStrategyContext, Task> GetStrategyExecuteDelegate(Type resultType)
        => _strategyExecuteDelegates.GetOrAdd(resultType, static rType =>
        {
            var method = typeof(ICommandStrategy)
                .GetMethod(nameof(ICommandStrategy.ExecuteAsync), 1, [typeof(CommandStrategyContext)])!
                .MakeGenericMethod(rType);
            var strategyParam = Expression.Parameter(typeof(ICommandStrategy), "strategy");
            var ctxParam = Expression.Parameter(typeof(CommandStrategyContext), "ctx");
            var call = Expression.Call(strategyParam, method, ctxParam);
            return Expression.Lambda<Func<ICommandStrategy, CommandStrategyContext, Task>>(
                Expression.Convert(call, typeof(Task)),
                strategyParam, ctxParam).Compile();
        });
}
