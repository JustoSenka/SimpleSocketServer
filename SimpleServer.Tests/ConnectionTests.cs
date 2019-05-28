using NUnit.Framework;
using SimpleServer.Client;
using SimpleServer.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleServer.Tests
{
    public class ConnectionTests
    {
        public static int s_TestRun = 0;
        public const int s_DefaultPort = 11000;
        public const string s_DefaultHost = "127.0.0.1";
        private static int GetFreePort() => s_DefaultPort + s_TestRun % 2;

        private IList<string> m_ServerLogs = new List<string>();
        private IList<string> m_ClientLogs = new List<string>();

        private void ServerLogReceivedCallback(string name, string msg) => m_ServerLogs.Add(msg);
        private void ClientLogReceivedCallback(string name, string msg) => m_ClientLogs.Add(msg);

        private Dictionary<string, IList<string>> m_DictLog = new Dictionary<string, IList<string>>();
        private void DictLogReceivedCallback(string name, string msg)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (!m_DictLog.ContainsKey(name))
                m_DictLog[name] = new List<string>();

            m_DictLog[name].Add(msg);
        }

        private SimpleSocketServer s;
        private SimpleSocketClient c;
        private SimpleSocketClient c1;
        private SimpleSocketClient c2;

        [SetUp]
        public void ClearMessages()
        {
            s_TestRun++;
            m_ServerLogs.Clear();
            m_ClientLogs.Clear();
            m_DictLog.Clear();
        }

        [TearDown]
        public void DisposeClients()
        {
            s?.Dispose();
            c?.Dispose();
            c1?.Dispose();
            c2?.Dispose();
        }

        [Test]
        public void Client_CanConnect_ToServer()
        {
            s = new SimpleSocketServer();
            var listen = s.StartListening(GetFreePort());

            c = new SimpleSocketClient("test");
            c.Connect(GetFreePort(), s_DefaultHost).Wait();

            Assert.IsTrue(c.IsConnected);
        }


        [Test]
        public void MultipleClients_CanConnect_ToServer()
        {
            s = new SimpleSocketServer();
            var listen = s.StartListening(GetFreePort());

            c = new SimpleSocketClient("test");
            c1 = new SimpleSocketClient("test1");
            c2 = new SimpleSocketClient("test2");

            Task.WaitAll(new Task<bool>[]
            {
                 c.Connect(GetFreePort(), s_DefaultHost),
                 c1.Connect(GetFreePort(), s_DefaultHost),
                 c2.Connect(GetFreePort(), s_DefaultHost)
            });

            Assert.IsTrue(c.IsConnected);
            Assert.IsTrue(c1.IsConnected);
            Assert.IsTrue(c2.IsConnected);
        }

        [Test]
        public void Server_GetsConnectionMessages_FromClient()
        {
            s = new SimpleSocketServer();
            s.MessageReceived += ServerLogReceivedCallback;
            var listen = s.StartListening(GetFreePort());

            c = new SimpleSocketClient("test");
            c1 = new SimpleSocketClient("test1");
            c2 = new SimpleSocketClient("test2");

            Task.WaitAll(new Task<bool>[]
            {
                 c.Connect(),
                 c1.Connect(),
                 c2.Connect()
            });

            CollectionAssert.AreEquivalent(new[] { "test", "test1", "test2" }, m_ServerLogs);
        }

        /*[Test]
        public void Server_GetsMessages_FromClient()
        {
            s = new SimpleSocketServer();
            s.MessageReceived += ServerLogReceivedCallback;
            var listen = s.StartListening();

            c = new SimpleSocketClient("test");
            c.MessageReceived += ClientLogReceivedCallback;
            c.Connect().Wait();


            Task.WaitAll(new Task<bool>[]
            {
                 c.Send("111"),
                 c.Send("222"),
                 c.Send("333")
            });

            s.Send("test", "response").Wait();

            c.Send("test").Wait();

            s.Dispose();
            c.Dispose();
        }*/
    }
}