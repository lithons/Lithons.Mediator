using Lithons.Mediator.Abstractions.Middleware.Command;
using System.Threading.Channels;

namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Represents an in-process channel used to enqueue commands for background processing.
/// </summary>
public interface ICommandsChannel
{
    /// <summary>
    /// Gets the write end of the channel used to enqueue commands.
    /// </summary>
    ChannelWriter<CommandContext> Writer { get; }

    /// <summary>
    /// Gets the read end of the channel used to consume commands.
    /// </summary>
    ChannelReader<CommandContext> Reader { get; }
}
