using Lithons.Mediator.Contracts;

namespace Lithons.Mediator.Options;

public class MediatorOptions
{
    public INotificationStrategy DefaultNotificationStrategy { get; set; } = NotificationStrategy.Sequential;
    public ICommandStrategy DefaultCommandStrategy { get; set; } = CommandStrategy.Default;
}
