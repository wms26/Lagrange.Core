using Laana;
using Lagrange.Core;
using Lagrange.Laana.Common;
using LiteDB;

namespace Lagrange.Laana.Action.Message
{
    [ActionHandlerOf(LaanaActionPing.PingOneofCase.GetMessage)]
    public sealed class GetMessageAction(ILiteDatabase db) : IAction<GetMessagePing, GetMessagePong>
    {
        public Task<GetMessagePong> Handle(BotContext bot, GetMessagePing ping)
        {
            var chatType = ping.Peer.Type switch
            {
                LaanaPeer.Types.Type.Group => MessageRecord.ChatType.Group,
                _ => MessageRecord.ChatType.Private
            };

            var record = db.GetCollection<MessageRecord>().Query()
                .Where(x => x.Type == chatType)
                .Where(x => x.PeerUin == ping.Peer.Uin)
                .Where(x => x.MsgSeq == ping.MsgSeq)
                .First();
            if (record == null)
            {
                throw new Exception("Message not found.");
            }

            return Task.FromResult(new GetMessagePong { Message = record.Restore() });
        }
    }
}