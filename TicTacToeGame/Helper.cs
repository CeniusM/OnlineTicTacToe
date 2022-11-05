/*

All of "Not my code"

*/

using System.Net; //Include this namespace
using System.Net.Sockets;

namespace TicTacToeGame;

internal class Helper
{
    // thanks StackOverflow
    public static string GetLocalIPAddressString()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}
