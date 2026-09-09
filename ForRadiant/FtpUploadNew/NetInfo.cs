using System.Net;
using System.Net.Sockets;

namespace FtpUpload;

/// <summary>
/// Which IP THIS machine uploads from.
///
/// A site runs ~200 of these, on more than one subnet (10.119.x and 10.121.x in production), and
/// the logs never said which machine produced them. When a panel is missing, "which PC was this?"
/// had to be answered from the folder the zip came from. Recording the client IP makes every log
/// self-identifying.
/// </summary>
public static class NetInfo
{
    private static string? _cached;
    private static string? _cachedFor;

    /// <summary>
    /// The local address used to reach <paramref name="targetHost"/>. Found by opening a UDP socket
    /// to the target and reading the socket's local endpoint: no packet is sent, but the OS picks
    /// the real outbound interface, which is what matters on a multi-homed machine. Falls back to
    /// the first non-loopback IPv4 address, then to "unknown".
    /// </summary>
    public static string LocalIp(string targetHost)
    {
        if (_cached is not null && _cachedFor == targetHost) return _cached;

        var ip = ViaRoute(targetHost) ?? FirstIPv4() ?? "unknown";
        _cached = ip;
        _cachedFor = targetHost;
        return ip;
    }

    private static string? ViaRoute(string targetHost)
    {
        if (string.IsNullOrWhiteSpace(targetHost)) return null;
        try
        {
            using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.Connect(targetHost, 65530);                 // UDP connect sends nothing
            return (s.LocalEndPoint as IPEndPoint)?.Address.ToString();
        }
        catch { return null; }                            // unresolvable/unreachable host
    }

    private static string? FirstIPv4()
    {
        try
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
                      .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork
                                        && !IPAddress.IsLoopback(a))
                      ?.ToString();
        }
        catch { return null; }
    }

    /// <summary>Machine name plus outbound IP, for a log line: "HNAMAL516L 10.119.211.42".</summary>
    public static string Describe(string targetHost)
    {
        var name = "";
        try { name = Environment.MachineName; } catch { }
        return string.IsNullOrEmpty(name) ? LocalIp(targetHost) : $"{name} {LocalIp(targetHost)}";
    }
}
