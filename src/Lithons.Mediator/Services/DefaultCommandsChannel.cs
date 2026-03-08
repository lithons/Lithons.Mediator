using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using System.Threading.Channels;

namespace Lithons.Mediator.Services;

/// <summary>
/// Default <see cref="ICommandsChannel"/> implementation backed by a <see cref="Channel{T}"/>.
/// </summary>
public sealed class DefaultCommandsChannel(Channel<CommandContext> channel) : ICommandsChannel
{
    /// <inheritdoc />
    public ChannelWriter<CommandContext> Writer => channel.Writer;

    /// <inheritdoc />
    public ChannelReader<CommandContext> Reader => channel.Reader;
}
