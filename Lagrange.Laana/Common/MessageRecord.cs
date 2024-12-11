using Laana;

namespace Lagrange.Laana.Common
{
    [Serializable]
    public class MessageRecord
    {
        public enum ChatType
        {
            Private = 0,
            Group = 1
        }
        
        public ulong MsgSeq { get; set; }
        
        public ulong Time { get; set; }
        
        public string SenderUin { get; set; }
        
        public ChatType Type { get; set; }
        
        public string PeerUin { get; set; }
        
        public LaanaMessage.ContentOneofCase ContentCase { get; set; }
        
        public byte[] Content { get; set; }
    }
    
    public static class MessageRecordEx
    {
        public static LaanaMessage Restore(this MessageRecord record)
        {
            var message = new LaanaMessage
            {
                MsgSeq = record.MsgSeq,
                Time = record.Time,
                SenderUin = record.SenderUin,
                Peer = new LaanaPeer
                {
                    Type = record.Type switch
                    {
                        MessageRecord.ChatType.Group => LaanaPeer.Types.Type.Group,
                        _ => LaanaPeer.Types.Type.Buddy
                    },
                    Uin = record.PeerUin
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
            
            return message;
        }
    }
}