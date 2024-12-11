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
    public sealed class SendMessageAction(IOutgoingMessageConverter converter, ILiteDatabase db) : IAction<SendMessagePing, SendMessagePong>
    {
        public async Task<SendMessagePong> Handle(BotContext bot, SendMessagePing ping)
        {
            var messageResult = await bot.SendMessage(await converter.Convert(ping.Message, ping.TargetPeer));
            if (messageResult.Result != 0)
            {
                throw new Exception($"Failed to send message. Error code: {messageResult.Result}");
            }

            if (messageResult.Sequence is null or 0)
            {
                throw new Exception("Failed to send message. No sequence returned. Error code: 9000");
            }

            var messageRecord = new MessageRecord
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

            switch (ping.Message.ContentCase)
            {
                case LaanaOutgoingMessage.ContentOneofCase.Bubble:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.Bubble;
                    messageRecord.Content = ping.Message.Bubble.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.File:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.File;
                    messageRecord.Content = ping.Message.File.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.SingleImage:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.SingleImage;
                    messageRecord.Content = ping.Message.SingleImage.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.MarketFace:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.MarketFace;
                    messageRecord.Content = ping.Message.MarketFace.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.Video:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.Video;
                    messageRecord.Content = ping.Message.Video.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.Voice:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.Voice;
                    messageRecord.Content = ping.Message.Voice.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.LinkCard:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.LinkCard;
                    messageRecord.Content = ping.Message.LinkCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ContactCard:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.ContactCard;
                    messageRecord.Content = ping.Message.ContactCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.MusicCard:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.MusicCard;
                    messageRecord.Content = ping.Message.MusicCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.LocationCard:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.LocationCard;
                    messageRecord.Content = ping.Message.LocationCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ForwardedMessage:
                    // Because it is constructed (fake), there is no reason to store it as-is.
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.ExtendedMessage;
                    messageRecord.Content = new LaanaMessage.Types.ExtendedMessage
                    {
                        Type = "ConstructedForwardedMessage", 
                        Content = ByteString.CopyFrom(ping.Message.ForwardedMessage.Serialize())
                    }.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.XmlMessage:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.XmlMessage;
                    messageRecord.Content = ping.Message.XmlMessage.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ArkMessage:
                    messageRecord.ContentCase = LaanaMessage.ContentOneofCase.ArkMessage;
                    messageRecord.Content = ping.Message.ArkMessage.Serialize().ToArray();
                    break;
                default:
                    throw new Exception("Unexpected message content type.");
            }

            db.GetCollection<MessageRecord>().Insert(messageRecord);
            
            return new SendMessagePong { MsgSeq = (ulong)messageResult.Sequence! };
        }
    }
}