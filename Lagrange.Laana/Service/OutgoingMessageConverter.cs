using Laana;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.Laana.Service
{
    public interface IOutgoingMessageConverter
    {
        MessageChain ToMessageChain(LaanaOutgoingMessage outgoingMessage, LaanaPeer targetPeer);
    }
    
    public sealed class OutgoingMessageConverter(ICacheManager cacheManager) : IOutgoingMessageConverter
    {
        public MessageChain ToMessageChain(LaanaOutgoingMessage outgoingMessage, LaanaPeer targetPeer)
        {
            var builder = targetPeer.Type switch
            {
                LaanaPeer.Types.Type.Buddy => MessageBuilder.Friend(uint.Parse(targetPeer.Uin)),
                LaanaPeer.Types.Type.Group => MessageBuilder.Group(uint.Parse(targetPeer.Uin)),
                _ => throw new Exception("Invalid peer type.")
            };

            switch (outgoingMessage.ContentCase)
            {
                case LaanaOutgoingMessage.ContentOneofCase.Bubble:
                    AddLaanaBubble(builder, outgoingMessage.Bubble);
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.File:
                    throw new Exception("Use UploadXxxFile instead.");
                case LaanaOutgoingMessage.ContentOneofCase.SingleImage:
                case LaanaOutgoingMessage.ContentOneofCase.Video:
                case LaanaOutgoingMessage.ContentOneofCase.Voice:
                    // TODO: Resolve Laana File
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.MarketFace:
                    AddLaanaMarketFace(builder, outgoingMessage.MarketFace);
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.XmlMessage:
                    builder.Xml(outgoingMessage.XmlMessage.Xml);
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ForwardedMessage:
                    // TODO: Intercept that before calling this method. On done, delete this case.
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ArkMessage:
                case LaanaOutgoingMessage.ContentOneofCase.LinkCard:
                case LaanaOutgoingMessage.ContentOneofCase.ContactCard:
                case LaanaOutgoingMessage.ContentOneofCase.LocationCard:
                case LaanaOutgoingMessage.ContentOneofCase.MusicCard:
                case LaanaOutgoingMessage.ContentOneofCase.None:
                    throw new Exception("Unsupported message content type.");
                default:
                    throw new Exception("Invalid message content type.");
            }

            return builder.Build();
        }

        private void AddLaanaBubble(MessageBuilder builder, LaanaMessage.Types.Bubble bubble)
        {
            if (bubble.RepliedMsgSeq != 0)
            {
                builder.Add(new ForwardEntity { Sequence = (uint)bubble.RepliedMsgSeq });
            }

            foreach (var segment in bubble.Segments)
            {
                switch (segment.ContentCase)
                {
                    case LaanaMessage.Types.Bubble.Types.Segment.ContentOneofCase.Text:
                        builder.Text(segment.Text);
                        break;
                    case LaanaMessage.Types.Bubble.Types.Segment.ContentOneofCase.Face:
                        builder.Face((ushort)segment.Face);
                        break;
                    case LaanaMessage.Types.Bubble.Types.Segment.ContentOneofCase.At:
                        builder.Mention(uint.Parse(segment.At.Uin), segment.At.Name);
                        break;
                    case LaanaMessage.Types.Bubble.Types.Segment.ContentOneofCase.Image:
                        // TODO: Resolve Laana File
                        break;
                    case LaanaMessage.Types.Bubble.Types.Segment.ContentOneofCase.None:
                    default:
                        throw new Exception("Invalid segment content type.");
                }
            }
        }

        private void AddLaanaMarketFace(MessageBuilder builder, LaanaMessage.Types.MarketFace marketFace)
        {
            builder.MarketFace(
                marketFace.FaceId,
                (int)marketFace.FacePackageId,
                marketFace.FaceKey,
                marketFace.DisplayText);
        }
    }
}