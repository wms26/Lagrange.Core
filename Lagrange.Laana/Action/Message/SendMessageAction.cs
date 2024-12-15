using Google.Protobuf;
using Laana;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Utility.Extension;
using Lagrange.Laana.Common;
using Lagrange.Laana.Service;
using LiteDB;

namespace Lagrange.Laana.Action.Message
{
    [ActionHandlerOf(LaanaActionPing.PingOneofCase.SendMessage)]
    public sealed class SendMessageAction(IOutgoingMessageConvertService convertService, ILiteDatabase db) : IAction<SendMessagePing, SendMessagePong>
    {
        public async Task<SendMessagePong> Handle(BotContext bot, SendMessagePing ping)
        {
            var messageResult = await bot.SendMessage(await convertService.Convert(ping.Message, ping.TargetPeer));
            if (messageResult.Result != 0)
            {
                throw new Exception($"Failed to send message. Error code: {messageResult.Result}");
            }

            if (messageResult.Sequence is null or 0)
            {
                throw new Exception("Failed to send message. No sequence returned. Error code: 9000");
            }

            var messageRecord = new MessageRecord(ping.Message)
            {
                MsgSeq = (ulong)messageResult.Sequence,
                Time = messageResult.Timestamp * 1000,
                SenderUin = bot.BotUin.ToString(),
                Type = ping.TargetPeer.Type switch
                {
                    LaanaPeer.Types.Type.Group => MessageRecord.ChatType.Group,
                    _ => MessageRecord.ChatType.Private
                },
                PeerUin = ping.TargetPeer.Uin
            };

            db.GetCollection<MessageRecord>().Insert(messageRecord);
            
            return new SendMessagePong { MsgSeq = (ulong)messageResult.Sequence! };
        }
    }
}