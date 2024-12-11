using Laana;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.Laana.Common
{
    public static class LagrangeMessageEx
    {
        public static LaanaMessage ToLaanaMessage(this MessageChain incomingMessage)
        {
            LaanaMessage message = new()
            {
                MsgSeq = incomingMessage.Sequence,
                Time = (ulong)new DateTimeOffset(incomingMessage.Time).ToUnixTimeMilliseconds(),
                SenderUin = incomingMessage.FriendUin.ToString(),
                Peer = incomingMessage.Type switch
                {
                    MessageChain.MessageType.Group => new LaanaPeer
                    {
                        Type = LaanaPeer.Types.Type.Group, Uin = incomingMessage.GroupUin.ToString()
                    },
                    _ => new LaanaPeer // Currently regard temporary chat as buddy chat
                    {
                        Type = LaanaPeer.Types.Type.Buddy, Uin = incomingMessage.FriendUin.ToString()
                    }
                }
            };

            if (incomingMessage.Count == 0)
            {
                return null;
            }

            var firstEntity = incomingMessage[0];
            switch (firstEntity)
            {
                case TextEntity or MentionEntity or FaceEntity or ForwardEntity:
                    message.Bubble = incomingMessage.ToLaanaBubble();
                    break;
                case ImageEntity imageEntity when incomingMessage.Count == 1:
                    message.SingleImage = imageEntity.ToLaanaSingleImage();
                    break;
                case ImageEntity:
                    message.Bubble = incomingMessage.ToLaanaBubble();
                    break;
                case FileEntity fileEntity:
                    message.File = fileEntity.ToLaanaNormalFile();
                    break;
                case MarketfaceEntity marketFaceEntity:
                    message.MarketFace = marketFaceEntity.ToLaanaMarketFace();
                    break;
                case VideoEntity videoEntity:
                    message.Video = videoEntity.ToLaanaVideo();
                    break;
                case RecordEntity recordEntity:
                    message.Voice = recordEntity.ToLaanaVoice();
                    break;
                case MultiMsgEntity multiMsgEntity:
                    message.ForwardedMsgRef = multiMsgEntity.ToLaanaForwardedMessageRef();
                    break;
                case XmlEntity xmlEntity:
                    message.XmlMessage = xmlEntity.ToLaanaXmlMessage();
                    break;
                case JsonEntity jsonEntity:
                    message.ArkMessage = jsonEntity.ToLaanaArkMessage();
                    break;
            }

            return message;
        }

        private static LaanaMessage.Types.Bubble ToLaanaBubble(this MessageChain incomingMessage)
        {
            var bubble = new LaanaMessage.Types.Bubble();

            int startingIndex = 0;
            if (incomingMessage[0] is ForwardEntity forwardEntity)
            {
                startingIndex = 1;
                bubble.RepliedMsgSeq = forwardEntity.Sequence;
            }

            for (int i = startingIndex; i < incomingMessage.Count; i++)
            {
                bubble.Segments.Add(incomingMessage[i] switch
                {
                    TextEntity textEntity => new LaanaMessage.Types.Bubble.Types.Segment { Text = textEntity.Text },
                    FaceEntity faceEntity => new LaanaMessage.Types.Bubble.Types.Segment { Face = faceEntity.FaceId },
                    MentionEntity mentionEntity => new LaanaMessage.Types.Bubble.Types.Segment
                    {
                        At = new LaanaMessage.Types.Bubble.Types.Segment.Types.At
                        {
                            Name = mentionEntity.Name, Uin = mentionEntity.Uin.ToString()
                        }
                    },
                    ImageEntity imageEntity => new LaanaMessage.Types.Bubble.Types.Segment
                    {
                        Image = new LaanaFile { Url = imageEntity.ImageUrl }
                    },
                    _ => throw new Exception("Invalid entity type.")
                });
            }

            return bubble;
        }

        private static LaanaMessage.Types.SingleImage ToLaanaSingleImage(this ImageEntity imageEntity)
        {
            return new LaanaMessage.Types.SingleImage
            {
                Image = new LaanaFile { Url = imageEntity.ImageUrl },
                DisplayText = imageEntity.SubType == 0 ? "[图片]" : "[动画表情]"
            };
        }

        private static LaanaMessage.Types.NormalFile ToLaanaNormalFile(this FileEntity fileEntity)
        {
            return new LaanaMessage.Types.NormalFile
            {
                File = new LaanaFile { Url = fileEntity.FileUrl ?? "" },
                Name = fileEntity.FileName,
                Size = (ulong)fileEntity.FileSize
            };
        }

        private static LaanaMessage.Types.MarketFace ToLaanaMarketFace(this MarketfaceEntity marketfaceEntity)
        {
            return new LaanaMessage.Types.MarketFace
            {
                FacePackageId = (uint)marketfaceEntity.EmojiPackageId,
                FaceId = marketfaceEntity.EmojiId,
                FaceKey = marketfaceEntity.Key,
                DisplayText = marketfaceEntity.Summary
            };
        }

        private static LaanaMessage.Types.Video ToLaanaVideo(this VideoEntity videoEntity)
        {
            return new LaanaMessage.Types.Video
            {
                Video_ = new LaanaFile { Url = videoEntity.VideoUrl }, Duration = (uint)videoEntity.VideoLength
            };
        }

        private static LaanaMessage.Types.Voice ToLaanaVoice(this RecordEntity recordEntity)
        {
            return new LaanaMessage.Types.Voice
            {
                Voice_ = new LaanaFile { Url = recordEntity.AudioUrl }, Duration = (uint)recordEntity.AudioLength
            };
        }

        private static LaanaMessage.Types.ForwardedMessageRef ToLaanaForwardedMessageRef(
            this MultiMsgEntity multiMsgEntity)
        {
            // TODO: multi msg chains are actively loaded on receiving, so refactoring is needed
            return new LaanaMessage.Types.ForwardedMessageRef
            {
                RefId = multiMsgEntity.ResId, DisplayText = multiMsgEntity.DetailStr
            };
        }

        private static LaanaMessage.Types.XmlMessage ToLaanaXmlMessage(this XmlEntity xmlEntity)
        {
            return new LaanaMessage.Types.XmlMessage { Xml = xmlEntity.Xml };
        }

        private static LaanaMessage.Types.ArkMessage ToLaanaArkMessage(this JsonEntity jsonEntity)
        {
            return new LaanaMessage.Types.ArkMessage { Json = jsonEntity.Json };
        }
    }
}