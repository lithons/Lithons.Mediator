using Lithons.Mediator.Contracts;
using Lithons.Mediator.Contexts;
using Lithons.Mediator.Middleware.Notification;
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
}
