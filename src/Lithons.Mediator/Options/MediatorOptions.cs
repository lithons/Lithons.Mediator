using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Options;

/// <summary>
/// Configuration options applied to the mediator at startup.
/// </summary>
public class MediatorOptions
{
    /// <summary>
    /// Gets or sets the default strategy used when publishing notifications.
    /// </summary>
    public INotificationStrategy DefaultNotificationStrategy { get; set; } = NotificationStrategy.Sequential;

    /// <summary>
    /// Gets or sets the default strategy used when sending commands.
    /// </summary>
    public ICommandStrategy DefaultCommandStrategy { get; set; } = CommandStrategy.Default;
}
