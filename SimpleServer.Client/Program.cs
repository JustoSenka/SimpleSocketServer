using SimpleServer.Server;
using System;
using System.Threading.Tasks;

namespace SimpleServer.Client
{
    public class Program
    {
        public static int Main(String[] args)
        {
            var s = new SimpleSocketServer();
            var t = s.StartListening();

            var c = new SimpleSocketClient("test");
            c.Connect();

            var t = new Task<bool>[]
            {
            c.Send("test data"),
            c.Send("1111111111111111111"),
            c.Send("222222222222222222"),
            c.Send("33333333333333333"),
            c.Send("4444444444444444"),
            c.Send("5555555555555"),
            c.Send("66666666666"),
            c.Send("77777777777"),
            c.Send("8888888888")
            };

            Task.WaitAll(t);
            c.Dispose();
            return 0;
        }
    }
}
