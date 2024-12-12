using Google.Protobuf;
using Laana;
using Lagrange.Core.Utility.Extension;

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
        
        public MessageRecord(LaanaOutgoingMessage laanaOutgoingMessage)
        {
            switch (laanaOutgoingMessage.ContentCase)
            {
                case LaanaOutgoingMessage.ContentOneofCase.Bubble:
                    ContentCase = LaanaMessage.ContentOneofCase.Bubble;
                    Content = laanaOutgoingMessage.Bubble.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.File:
                    ContentCase = LaanaMessage.ContentOneofCase.File;
                    Content = laanaOutgoingMessage.File.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.SingleImage:
                    ContentCase = LaanaMessage.ContentOneofCase.SingleImage;
                    Content = laanaOutgoingMessage.SingleImage.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.MarketFace:
                    ContentCase = LaanaMessage.ContentOneofCase.MarketFace;
                    Content = laanaOutgoingMessage.MarketFace.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.Video:
                    ContentCase = LaanaMessage.ContentOneofCase.Video;
                    Content = laanaOutgoingMessage.Video.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.Voice:
                    ContentCase = LaanaMessage.ContentOneofCase.Voice;
                    Content = laanaOutgoingMessage.Voice.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.LinkCard:
                    ContentCase = LaanaMessage.ContentOneofCase.LinkCard;
                    Content = laanaOutgoingMessage.LinkCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ContactCard:
                    ContentCase = LaanaMessage.ContentOneofCase.ContactCard;
                    Content = laanaOutgoingMessage.ContactCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.MusicCard:
                    ContentCase = LaanaMessage.ContentOneofCase.MusicCard;
                    Content = laanaOutgoingMessage.MusicCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.LocationCard:
                    ContentCase = LaanaMessage.ContentOneofCase.LocationCard;
                    Content = laanaOutgoingMessage.LocationCard.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ForwardedMessage:
                    // Because it is constructed (fake), there is no reason to store it as-is.
                    ContentCase = LaanaMessage.ContentOneofCase.ExtendedMessage;
                    Content = new LaanaMessage.Types.ExtendedMessage
                    {
                        Type = "ConstructedForwardedMessage", 
                        Content = ByteString.CopyFrom(laanaOutgoingMessage.ForwardedMessage.Serialize())
                    }.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.XmlMessage:
                    ContentCase = LaanaMessage.ContentOneofCase.XmlMessage;
                    Content = laanaOutgoingMessage.XmlMessage.Serialize().ToArray();
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ArkMessage:
                    ContentCase = LaanaMessage.ContentOneofCase.ArkMessage;
                    Content = laanaOutgoingMessage.ArkMessage.Serialize().ToArray();
                    break;
                default:
                    throw new Exception("Unexpected message content type.");
            }
        }

        public LaanaMessage Restore()
        {
            var message = new LaanaMessage
            {
                MsgSeq = MsgSeq,
                Time = Time,
                SenderUin = SenderUin,
                Peer = new LaanaPeer
                {
                    Type = Type switch
                    {
                        ChatType.Group => LaanaPeer.Types.Type.Group,
                        _ => LaanaPeer.Types.Type.Buddy
                    },
                    Uin = PeerUin
                }
            };
            
            switch (ContentCase)
            {
                case LaanaMessage.ContentOneofCase.Bubble:
                    message.Bubble = LaanaMessage.Types.Bubble.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.File:
                    message.File = LaanaMessage.Types.NormalFile.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.SingleImage:
                    message.SingleImage = LaanaMessage.Types.SingleImage.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.MarketFace:
                    message.MarketFace = LaanaMessage.Types.MarketFace.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.Video:
                    message.Video = LaanaMessage.Types.Video.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.Voice:
                    message.Voice = LaanaMessage.Types.Voice.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.LinkCard:
                    message.LinkCard = LaanaMessage.Types.LinkCard.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.ContactCard:
                    message.ContactCard = LaanaMessage.Types.ContactCard.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.MusicCard:
                    message.MusicCard = LaanaMessage.Types.MusicCard.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.LocationCard:
                    message.LocationCard = LaanaMessage.Types.LocationCard.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.XmlMessage:
                    message.XmlMessage = LaanaMessage.Types.XmlMessage.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.ArkMessage:
                    message.ArkMessage = LaanaMessage.Types.ArkMessage.Parser.ParseFrom(Content);
                    break;
                case LaanaMessage.ContentOneofCase.ExtendedMessage:
                    message.ExtendedMessage = LaanaMessage.Types.ExtendedMessage.Parser.ParseFrom(Content);
                    break;
                default:
                    throw new Exception("Unexpected message content type.");
            }
            
            return message;
        }
    }
}