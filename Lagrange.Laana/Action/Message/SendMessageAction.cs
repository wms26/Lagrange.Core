using Laana;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Laana.Common;

namespace Lagrange.Laana.Action.Message;

[ActionHandlerOf(LaanaActionPing.PingOneofCase.SendMessage)]
public class SendMessageAction : IAction<SendMessagePing, SendMessagePong>
{
    public async Task<SendMessagePong> Handle(BotContext bot, SendMessagePing ping)
    {
        var messageResult = await bot.SendMessage(ping.Message.ToMessageChain(ping.TargetPeer));
        if (messageResult.Result != 0)
        {
            throw new Exception($"Failed to send message. Error code: {messageResult.Result}");
        }
        if (messageResult.Sequence is null or 0)
        {
            throw new Exception("Failed to send message. No sequence returned. Error code: 9000");
        }
        // TODO: Save this message to database
        return new SendMessagePong { MsgSeq = (ulong)messageResult.Sequence! };
    }
}