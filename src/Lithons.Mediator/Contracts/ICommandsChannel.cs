using Lithons.Mediator.Middleware.Command;
using System.Threading.Channels;

namespace Lithons.Mediator.Contracts;

public interface ICommandsChannel
{
    ChannelWriter<CommandContext> Writer { get; }
    ChannelReader<CommandContext> Reader { get; }
}
