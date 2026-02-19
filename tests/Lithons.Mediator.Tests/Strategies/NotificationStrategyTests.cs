using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.NotificationStrategies;
using Microsoft.Extensions.Logging;

namespace Lithons.Mediator.Tests.Strategies;

public class NotificationStrategyTests
{
    private record TestNotification(string Message) : INotification;

    private class TrackingHandler : INotificationHandler<TestNotification>
    {
        public List<string> Received { get; } = [];
        public int CallOrder { get; private set; }
        private static int _counter;

        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
        {
            CallOrder = Interlocked.Increment(ref _counter);
            Received.Add(notification.Message);
            return Task.CompletedTask;
        }
    }

    private static NotificationStrategyContext BuildContext(
        INotification notification,
        INotificationStrategy strategy,
        INotificationHandler[] handlers,
        CancellationToken cancellationToken = default)
    {
        var sp = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<ILogger>();
        var notifContext = new NotificationContext(notification, strategy, sp, cancellationToken);
        return new NotificationStrategyContext(notifContext, handlers, logger, sp);
    }

    [Fact]
    public async Task SequentialStrategy_PublishAsync_InvokesAllHandlers()
    {
        var handler1 = new TrackingHandler();
        var handler2 = new TrackingHandler();
        var notification = new TestNotification("ping");
        var strategy = new SequentialStrategy();
        var context = BuildContext(notification, strategy, [handler1, handler2], TestContext.Current.CancellationToken);

        await strategy.PublishAsync(context);

        Assert.Equal(["ping"], handler1.Received);
        Assert.Equal(["ping"], handler2.Received);
    }

    [Fact]
    public async Task SequentialStrategy_PublishAsync_InvokesHandlersInOrder()
    {
        var completionOrder = new List<int>();
        var handler1 = new DelayedHandler(50, 1, completionOrder);
        var handler2 = new DelayedHandler(0, 2, completionOrder);
        var notification = new TestNotification("ping");
        var strategy = new SequentialStrategy();
        var context = BuildContext(notification, strategy, [handler1, handler2], TestContext.Current.CancellationToken);

        await strategy.PublishAsync(context);

        Assert.Equal([1, 2], completionOrder);
    }

    [Fact]
    public async Task ParallelStrategy_PublishAsync_InvokesAllHandlers()
    {
        var handler1 = new TrackingHandler();
        var handler2 = new TrackingHandler();
        var notification = new TestNotification("ping");
        var strategy = new ParallelStrategy();
        var context = BuildContext(notification, strategy, [handler1, handler2], TestContext.Current.CancellationToken);

        await strategy.PublishAsync(context);

        Assert.Equal(["ping"], handler1.Received);
        Assert.Equal(["ping"], handler2.Received);
    }

    [Fact]
    public async Task ParallelStrategy_PublishAsync_NoHandlers_DoesNotThrow()
    {
        var notification = new TestNotification("ping");
        var strategy = new ParallelStrategy();
        var context = BuildContext(notification, strategy, [], TestContext.Current.CancellationToken);

        await strategy.PublishAsync(context);
    }

    [Fact]
    public async Task SequentialStrategy_PublishAsync_NoHandlers_DoesNotThrow()
    {
        var notification = new TestNotification("ping");
        var strategy = new SequentialStrategy();
        var context = BuildContext(notification, strategy, [], TestContext.Current.CancellationToken);

        await strategy.PublishAsync(context);
    }

    private class DelayedHandler(int delayMs, int id, List<int> order) : INotificationHandler<TestNotification>
    {
        public async Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(delayMs, cancellationToken);
            order.Add(id);
        }
    }

    private class ThrowingHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
            => throw new InvalidOperationException("handler error");
    }

    [Fact]
    public async Task SequentialStrategy_HandlerThrows_PropagatesException()
    {
        var handler = new ThrowingHandler();
        var notification = new TestNotification("ping");
        var strategy = new SequentialStrategy();
        var context = BuildContext(notification, strategy, [handler], TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategy.PublishAsync(context));
    }

    [Fact]
    public async Task SequentialStrategy_HandlerThrows_RemainingHandlersNotInvoked()
    {
        var tracking = new TrackingHandler();
        var notification = new TestNotification("ping");
        var strategy = new SequentialStrategy();
        var context = BuildContext(notification, strategy, [new ThrowingHandler(), tracking], TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategy.PublishAsync(context));

        Assert.Empty(tracking.Received);
    }

    [Fact]
    public async Task ParallelStrategy_HandlerThrows_PropagatesException()
    {
        var handler = new ThrowingHandler();
        var notification = new TestNotification("ping");
        var strategy = new ParallelStrategy();
        var context = BuildContext(notification, strategy, [handler], TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategy.PublishAsync(context));
    }

    [Fact]
    public async Task ParallelStrategy_HandlersRunConcurrently()
    {
        var tcs1 = new TaskCompletionSource();
        var tcs2 = new TaskCompletionSource();
        var concurrentHandler1 = new GateHandler(tcs1, tcs2);
        var concurrentHandler2 = new GateHandler(tcs2, tcs1);
        var notification = new TestNotification("ping");
        var strategy = new ParallelStrategy();
        var context = BuildContext(notification, strategy, [concurrentHandler1, concurrentHandler2], TestContext.Current.CancellationToken);

        // Both handlers unblock each other, so this only completes if they run concurrently
        await strategy.PublishAsync(context);

        Assert.True(concurrentHandler1.Completed);
        Assert.True(concurrentHandler2.Completed);
    }

    private class GateHandler(TaskCompletionSource signal, TaskCompletionSource waitFor) : INotificationHandler<TestNotification>
    {
        public bool Completed { get; private set; }

        public async Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
        {
            signal.TrySetResult();
            await waitFor.Task.WaitAsync(cancellationToken);
            Completed = true;
        }
    }
}
