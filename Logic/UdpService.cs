using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace TiltControl.Logic
{
    public class UdpService
    {
        private UdpClient _UdpClient;
        private string _ipAddress;
        private int _port;

        public UdpService(string ip, int port)
        {
            _ipAddress = ip;
            _port = port;
            _UdpClient = new UdpClient();
        }

        public async Task SendPacketAsync(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                await _UdpClient.SendAsync(data, data.Length, _ipAddress, _port);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"UDP Error: {ex.Message}");
            }
        }
    }
}
