namespace Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

/// <summary>
/// Represents the command middleware pipeline used to process command messages.
/// </summary>
public interface ICommandPipeline
{
    /// <summary>
    /// Configures the pipeline using the provided builder action and returns the composed delegate.
    /// </summary>
    /// <param name="setup">An action that adds middleware components to the pipeline builder.</param>
    /// <returns>The composed <see cref="CommandMiddlewareDelegate"/>.</returns>
    CommandMiddlewareDelegate Setup(Action<ICommandPipelineBuilder> setup);

    /// <summary>
    /// Gets the composed middleware delegate, building the default pipeline on first access.
    /// </summary>
    CommandMiddlewareDelegate Pipeline { get; }

    /// <summary>
    /// Executes the pipeline for the given context.
    /// </summary>
    /// <param name="context">The context carrying the command and its execution state.</param>
    Task InvokeAsync(CommandContext context);
}
