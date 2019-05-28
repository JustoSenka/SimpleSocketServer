using SimpleServer.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleServer.Client
{
    public class SimpleSocketClient : SimpleSocketBase, ISimpleSocketClient, IDisposable
    {
        public const int s_DefaultPort = 11000;
        public const string s_DefaultHost = "127.0.0.1";

        private SocketStateObject m_ClientState = new SocketStateObject();

        public bool IsConnected => m_ClientState.Socket.Connected;

        public SimpleSocketClient(string name)
        {
            m_ClientState.Name = name;
            Name = name;
        }

        public async Task<bool> Connect() => await Connect(s_DefaultPort, s_DefaultHost);
        public async Task<bool> Connect(int port, string host)
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                m_ClientState.Socket = socket;
                m_ClientState.Socket.Connect(remoteEP);

                Console.WriteLine($"[{Name}] {m_ClientState.Name} connected to server at: {socket.RemoteEndPoint.ToString()}");

                await Send(m_ClientState.Name);
                StartReceivingThread(m_ClientState);

                return m_ClientState.Socket.Connected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private void StartReceivingThread(SocketStateObject state)
        {
            new Thread(() =>
            {
                while (state.Socket.Connected)
                    Receive(state).Wait();

            }).Start();
        }

        public async Task<bool> Send(string msg) => await Send(m_ClientState, msg);

        public void Dispose()
        {
            try
            {
                m_ClientState.Socket.Shutdown(SocketShutdown.Both);
                m_ClientState.Socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
