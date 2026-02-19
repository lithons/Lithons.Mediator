using Lithons.Mediator.Abstractions.Middleware.Command;
using System.Threading.Channels;

namespace Lithons.Mediator.Abstractions.Contracts;

public interface ICommandsChannel
{
    ChannelWriter<CommandContext> Writer { get; }
    ChannelReader<CommandContext> Reader { get; }
}
