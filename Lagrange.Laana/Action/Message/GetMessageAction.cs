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
            
            var message = new LaanaMessage
            {
                MsgSeq = record.MsgSeq,
                Time = record.Time,
                SenderUin = record.SenderUin,
                Peer = new LaanaPeer
                {
                    Type = ping.Peer.Type,
                    Uin = ping.Peer.Uin
                }
            };
            
            switch (record.ContentCase)
            {
                case LaanaMessage.ContentOneofCase.Bubble:
                    message.Bubble = LaanaMessage.Types.Bubble.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.File:
                    message.File = LaanaMessage.Types.NormalFile.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.SingleImage:
                    message.SingleImage = LaanaMessage.Types.SingleImage.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.MarketFace:
                    message.MarketFace = LaanaMessage.Types.MarketFace.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.Video:
                    message.Video = LaanaMessage.Types.Video.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.Voice:
                    message.Voice = LaanaMessage.Types.Voice.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.LinkCard:
                    message.LinkCard = LaanaMessage.Types.LinkCard.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.ContactCard:
                    message.ContactCard = LaanaMessage.Types.ContactCard.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.MusicCard:
                    message.MusicCard = LaanaMessage.Types.MusicCard.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.LocationCard:
                    message.LocationCard = LaanaMessage.Types.LocationCard.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.XmlMessage:
                    message.XmlMessage = LaanaMessage.Types.XmlMessage.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.ArkMessage:
                    message.ArkMessage = LaanaMessage.Types.ArkMessage.Parser.ParseFrom(record.Content);
                    break;
                case LaanaMessage.ContentOneofCase.ExtendedMessage:
                    message.ExtendedMessage = LaanaMessage.Types.ExtendedMessage.Parser.ParseFrom(record.Content);
                    break;
                default:
                    throw new Exception("Unexpected message content type.");
            }
            
            return Task.FromResult(new GetMessagePong
            {
                Message = message
            });
        }
    }
}