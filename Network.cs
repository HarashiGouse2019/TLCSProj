using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace TLCSProj.Net
{
    internal static class Network
    {
        internal static bool IsConnected => NetworkInterface.GetIsNetworkAvailable();
        internal static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }

        
    }
}