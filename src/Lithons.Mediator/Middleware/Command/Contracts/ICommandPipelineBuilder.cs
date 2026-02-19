namespace Lithons.Mediator.Middleware.Command.Contracts;

public interface ICommandPipelineBuilder
{
    IDictionary<string, object?> Properties { get; }
    IServiceProvider ApplicationServices { get; }
    ICommandPipelineBuilder Use(Func<CommandMiddlewareDelegate, CommandMiddlewareDelegate> middleware);
    CommandMiddlewareDelegate Build();
}
