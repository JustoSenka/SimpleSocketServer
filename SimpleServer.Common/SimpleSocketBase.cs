using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Common
{
    public abstract class SimpleSocketBase
    {
        public event Action<string, string> MessageReceived;
        protected string Name;

        protected virtual async Task<bool> Receive(SocketStateObject socketState)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var didReceiveFullMessage = false;
                    while (!didReceiveFullMessage)
                    {
                        var bytesRead = socketState.Socket.Receive(socketState.Buffer, 0, SocketStateObject.BufferSize, 0);
                        if (bytesRead == 0)
                            continue;

                        socketState.sb.Append(Encoding.UTF8.GetString(socketState.Buffer, 0, bytesRead));

                        var content = socketState.sb.ToString();
                        if (!content.Contains("<EOF>"))
                            continue;

                        var msgs = content.Split("<EOF>");
                        foreach (var m in msgs)
                        {
                            if (string.IsNullOrEmpty(m))
                                continue; // since <EOF> is at the end of string, so one split will always be empty

                            Console.WriteLine($"[{socketState.Name}] {socketState.Name} <- {m}");

                            socketState.LastMessage = content.Replace("<EOF>", "");
                            MessageReceived?.Invoke(socketState.Name, socketState.LastMessage);
                            socketState.sb.Clear();
                            didReceiveFullMessage = true;
                            // TODO: Last element might not had <EOF> but we still considered it as full message. Append it again back to string builder and ignore in callbacks
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{Name}][Error] Connection unexpectedly closed: " + e.ToString());
                    return false;
                }
            });
        }

        public virtual async Task<bool> Send(SocketStateObject socketState, string msg)
        {
            var tcs = new TaskCompletionSource<bool>();
            var bytes = Encoding.UTF8.GetBytes(msg + "<EOF>");

            try
            {
                socketState.Socket.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), (tcs, socketState));
                Console.WriteLine($"[{Name}] {socketState.Name} -> {msg}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{Name}][Error] Failed to send data to socket: " + e.ToString());
                tcs.SetResult(false);
            }

            return await tcs.Task;
        }

        protected virtual void SendCallback(IAsyncResult ar)
        {
            var (tcs, socketState) = ((TaskCompletionSource<bool>, SocketStateObject))ar.AsyncState;

            try
            {
                socketState.Socket.EndSend(ar);
                tcs.SetResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{Name}][Error] Connection unexpectedly closed: " + e.ToString());
                tcs.SetResult(false);
            }
        }
    }
}
