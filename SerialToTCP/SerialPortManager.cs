using System.IO.Ports;

namespace SerialToTCP
{
    public class SerialPortManager : IDisposable
    {
        private readonly SerialPort _serialPort;

        public SerialPortManager(string comPort)
        {
            _serialPort = new SerialPort(comPort)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None
            };
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    await _serialPort.BaseStream.WriteAsync(data.AsMemory(0, data.Length), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to serial port: {ex.Message}");
            }
        }

        public async Task<int> ReadAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    return await _serialPort.BaseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from serial port: {ex.Message}");
            }
            return 0;
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }

}
