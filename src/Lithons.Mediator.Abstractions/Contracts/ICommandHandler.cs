namespace Lithons.Mediator.Abstractions.Contracts;

public interface ICommandHandler;

public interface ICommandHandler<in TCommand, TResult> : ICommandHandler
    where TCommand : ICommand
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand> : ICommandHandler
    where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken cancellationToken);
}
