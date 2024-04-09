using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

namespace SerialToTCP
{
    public class TcpConnectionHandler
    {
        private readonly TcpListener _tcpListener;
        private readonly SerialPortManager _serialPortManager;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ConcurrentBag<Task> _clientTasks = new();

        public TcpConnectionHandler(IPAddress ipAddress, int port, SerialPortManager serialPortManager)
        {
            _tcpListener = new TcpListener(ipAddress, port);
            _serialPortManager = serialPortManager;
        }

        public async Task StartAsync()
        {
            _tcpListener.Start();
            Console.WriteLine("TCP Server started. Listening for incoming connections...");

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var clientTask = HandleClientAsync(tcpClient, _cancellationTokenSource.Token);
                    _clientTasks.Add(clientTask);

                    // Cleanup finished tasks
                    _clientTasks.Where(t => t.IsCompleted).ToList().ForEach(t => _clientTasks.TryTake(out t));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Accepting client exception: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Client connected: {tcpClient.Client.RemoteEndPoint}");
                using (tcpClient)
                {
                    var stream = tcpClient.GetStream();
                    var buffer = new byte[4096];
                    while (!cancellationToken.IsCancellationRequested && tcpClient.Connected)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead > 0)
                        {
                            await _serialPortManager.WriteAsync(buffer[0..bytesRead], cancellationToken);
                            // Optionally, implement the reverse direction
                        }
                    }
                }
                Console.WriteLine("Client disconnected.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Handling client exception: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            _tcpListener.Stop();
            await Task.WhenAll(_clientTasks).ContinueWith(t => _clientTasks.Clear());
        }
    }

}
