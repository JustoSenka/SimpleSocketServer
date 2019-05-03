using SimpleServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleServer.Server
{
    public class SimpleSocketServer : SimpleSocketBase, IDisposable
    {
        public static int s_Port = 11000;

        private Socket m_ListeningSocket;
        private Dictionary<string, SocketStateObject> m_ConnectedSockets = new Dictionary<string, SocketStateObject>();

        private bool m_Run = true;

        public SimpleSocketServer()
        {
            Name = "Server";
        }

        public async Task<bool> StartListening()
        {
            return await Task.Run(async () =>
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, s_Port);

                m_ListeningSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    m_ListeningSocket.Bind(localEndPoint);
                    m_ListeningSocket.Listen(100);
                    Console.WriteLine($"[{Name}] Server online...");

                    while (m_Run)
                    {
                        var socket = m_ListeningSocket.Accept();
                        Console.WriteLine($"[{Name}] Client connected: " + socket.RemoteEndPoint.ToString());

                        var state = new SocketStateObject { Socket = socket };

                        // Need to receive proper name. First message should always be the name

                        await Receive(state);
                        state.Name = state.LastMessage;

                        // Add socket state object to the dcitionary or override the old one
                        if (m_ConnectedSockets.ContainsKey(state.Name))
                        {
                            m_ConnectedSockets[state.Name].Socket.Shutdown(SocketShutdown.Both);
                            m_ConnectedSockets[state.Name].Socket.Close();
                            m_ConnectedSockets[state.Name] = state;
                        }
                        else
                            m_ConnectedSockets.Add(state.Name, state);

                        StartReceivingThread(state);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{Name}][Error] Listening soccket failed to begin accept: " + e.ToString());
                    return false;
                }
            });
        }

        private void StartReceivingThread(SocketStateObject state)
        {
            new Thread(() =>
            {
                while (state.Socket.Connected)
                    Receive(state).Wait();

            }).Start();
        }

        public async Task<bool> Send(string name, string msg)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (m_ConnectedSockets.ContainsKey(name))
            {
                await Send(m_ConnectedSockets[name], msg);
            }
            else
            {
                Console.WriteLine($"[{Name}][Error] Client with name '{name}' is not connected.");
                tcs.SetResult(false);
            }

            return await tcs.Task;
        }

        private void UpdateConnectedSockets()
        {
            for (int i = m_ConnectedSockets.Values.Count - 1; i >= 0; i--)
            {
                var el = m_ConnectedSockets.Values.ElementAt(i);
                if (!el.Socket.Connected)
                    m_ConnectedSockets.Remove(el.Name);
            }
        }

        public void Dispose()
        {
            try
            {
                m_Run = false;

                m_ListeningSocket.Shutdown(SocketShutdown.Both);
                m_ListeningSocket.Close();

                foreach (var s in m_ConnectedSockets.Values)
                {
                    s.Socket.Shutdown(SocketShutdown.Both);
                    s.Socket.Close();
                }
                m_ConnectedSockets.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{Name}][Error] " + e.ToString());
            }
        }
    }
}
