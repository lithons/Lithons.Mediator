namespace Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

public interface ICommandMiddleware
{
    ValueTask InvokeAsync(CommandContext context);
}
