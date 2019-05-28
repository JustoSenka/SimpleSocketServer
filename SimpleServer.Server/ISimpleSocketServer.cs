using System;
using System.Threading.Tasks;

namespace SimpleServer.Server
{
    public interface ISimpleSocketServer : IDisposable
    {
        event Action<string, string> MessageReceived;

        Task<bool> StartListening();
        Task<bool> StartListening(int port);

        Task<bool> Send(string name, string msg);
    }
}