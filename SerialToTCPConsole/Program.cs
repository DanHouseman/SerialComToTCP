using SerialToTCP;
using System.Net;

const string serialPortName = "COM3"; // Change as per your configuration
const int tcpPort = 5001;
var ipAddress = IPAddress.Any;

SerialPortManager serialPortManager = new(serialPortName);
TcpConnectionHandler tcpConnectionHandler = new(ipAddress, tcpPort, serialPortManager);

try
{
    Console.WriteLine("Initializing the Serial Port...");
    serialPortManager.Open();

    Console.WriteLine($"Starting TCP server on port {tcpPort}...");
    var serverTask = tcpConnectionHandler.StartAsync();

    Console.WriteLine("Press Enter to stop the server...");
    Console.ReadLine();

    await tcpConnectionHandler.StopAsync();
    Console.WriteLine("Server stopped.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    serialPortManager.Dispose();
}
