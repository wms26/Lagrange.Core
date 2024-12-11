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
}