using Laana;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.Laana.Service
{
    public interface IOutgoingMessageConvertService
    {
        Task<MessageChain> Convert(LaanaOutgoingMessage outgoingMessage, LaanaPeer targetPeer);
    }
    
    public sealed class OutgoingMessageConvertService(IFileCacheService fileCacheService) : IOutgoingMessageConvertService
    {
        public async Task<MessageChain> Convert(LaanaOutgoingMessage outgoingMessage, LaanaPeer targetPeer)
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
                    // TODO: Intercept -> UploadXxxFile
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.SingleImage:
                    AddLaanaSingleImage(builder, outgoingMessage.SingleImage);
                    break;
                
                // TODO: calculate duration
                case LaanaOutgoingMessage.ContentOneofCase.Video:
                    builder.Video(await fileCacheService.ResolveIncomingLaanaFile(outgoingMessage.Video));
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.Voice:
                    builder.Record(await fileCacheService.ResolveIncomingLaanaFile(outgoingMessage.Voice));
                    break;
                
                case LaanaOutgoingMessage.ContentOneofCase.MarketFace:
                    AddLaanaMarketFace(builder, outgoingMessage.MarketFace);
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.XmlMessage:
                    builder.Xml(outgoingMessage.XmlMessage.Xml);
                    break;
                case LaanaOutgoingMessage.ContentOneofCase.ForwardedMessage:
                    // TODO: Intercept -> Send "Fake" ForwardedMessage
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

        private async void AddLaanaBubble(MessageBuilder builder, LaanaMessage.Types.Bubble bubble)
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
                        builder.Image(await fileCacheService.ResolveIncomingLaanaFile(segment.Image));
                        break;
                    case LaanaMessage.Types.Bubble.Types.Segment.ContentOneofCase.None:
                    default:
                        throw new Exception("Invalid segment content type.");
                }
            }
        }

        private async void AddLaanaSingleImage(MessageBuilder builder, LaanaMessage.Types.SingleImage singleImage)
        {
            builder.Add(new ImageEntity(await fileCacheService.ResolveIncomingLaanaFile(singleImage.Image))
            {
                Summary = singleImage.DisplayText
            });
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