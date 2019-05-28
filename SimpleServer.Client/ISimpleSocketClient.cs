using System;
using System.Threading.Tasks;

namespace SimpleServer.Client
{
    public interface ISimpleSocketClient : IDisposable
    {
        event Action<string, string> MessageReceived;
        bool IsConnected { get; }

        Task<bool> Connect();
        Task<bool> Connect(int port, string host);

        Task<bool> Send(string msg);
    }
}