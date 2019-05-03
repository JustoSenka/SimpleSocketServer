using System.Net.Sockets;
using System.Text;

namespace SimpleServer.Common
{
    public class SocketStateObject
    {
        public const int BufferSize = 1024;

        public string Name;

        public Socket Socket = null;
        public byte[] Buffer = new byte[BufferSize];

        public string LastMessage;
        public StringBuilder sb = new StringBuilder();
    }
}
