using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TrayChat;

sealed class Peer(string id, string name, IPAddress address, int tcpPort)
{
    public string Id { get; } = id;
    public string Name { get; set; } = name;
    public IPAddress Address { get; set; } = address;
    public int TcpPort { get; set; } = tcpPort;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}

// T: "hello" (broadcast announce), "here" (reply to a new peer), "bye" (leaving), "msg" (chat over TCP)
record Packet(string T, string Id, string Name, int Port, string? Text = null);

/// <summary>
/// Discovers other instances on the LAN via UDP broadcast and exchanges chat messages over TCP.
/// </summary>
sealed class Messenger : IDisposable
{
    public const int DiscoveryPort = 45454;
    static readonly TimeSpan AnnounceInterval = TimeSpan.FromSeconds(3);
    static readonly TimeSpan PeerTimeout = TimeSpan.FromSeconds(10);

    readonly Dictionary<string, Peer> peers = new();
    readonly UdpClient udp = new();
    readonly TcpListener tcp = new(IPAddress.Any, 0);
    readonly System.Windows.Forms.Timer timer = new();
    bool closed;

    public string Id { get; } = Guid.NewGuid().ToString("N");
    public string Name { get; } = $"{Environment.MachineName} ({Environment.UserName})";
    public IEnumerable<Peer> Peers => peers.Values.OrderBy(p => p.Name);
    public event Action<Peer, string>? MessageReceived;

    int TcpPort => ((IPEndPoint)tcp.LocalEndpoint).Port;

    public void Start()
    {
        // Allow several instances on one machine (handy for testing); broadcasts reach all of them.
        udp.ExclusiveAddressUse = false;
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
        udp.EnableBroadcast = true;
        tcp.Start();

        _ = ReceiveDiscoveryLoop();
        _ = AcceptLoop();

        timer.Interval = (int)AnnounceInterval.TotalMilliseconds;
        timer.Tick += (_, _) => { Broadcast("hello"); PruneStalePeers(); };
        timer.Start();
        Broadcast("hello");
    }

    public Peer? Find(string id) => peers.GetValueOrDefault(id);

    public async Task SendAsync(Peer peer, string text)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(peer.Address, peer.TcpPort).WaitAsync(TimeSpan.FromSeconds(5));
        var stream = client.GetStream();
        await stream.WriteAsync(Encode("msg", text));
        await stream.FlushAsync();
    }

    byte[] Encode(string type, string? text = null) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Packet(type, Id, Name, TcpPort, text)) + "\n");

    void Broadcast(string type)
    {
        var bytes = Encode(type);
        foreach (var address in BroadcastAddresses().Distinct())
        {
            try { udp.Send(bytes, bytes.Length, new IPEndPoint(address, DiscoveryPort)); }
            catch (SocketException) { }
        }
    }

    static IEnumerable<IPAddress> BroadcastAddresses()
    {
        yield return IPAddress.Broadcast;
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up) continue;
            foreach (var ua in nic.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ua.Address)) continue;
                var ip = ua.Address.GetAddressBytes();
                var mask = ua.IPv4Mask.GetAddressBytes();
                for (int i = 0; i < ip.Length; i++) ip[i] |= (byte)~mask[i];
                yield return new IPAddress(ip);
            }
        }
    }

    Peer Upsert(Packet p, IPAddress address)
    {
        if (peers.TryGetValue(p.Id, out var peer))
        {
            peer.Name = p.Name;
            peer.Address = address;
            peer.TcpPort = p.Port;
            peer.LastSeen = DateTime.UtcNow;
        }
        else
        {
            peer = peers[p.Id] = new Peer(p.Id, p.Name, address, p.Port);
        }
        return peer;
    }

    void PruneStalePeers()
    {
        var cutoff = DateTime.UtcNow - PeerTimeout;
        foreach (var stale in peers.Values.Where(p => p.LastSeen < cutoff).ToList())
            peers.Remove(stale.Id);
    }

    async Task ReceiveDiscoveryLoop()
    {
        while (!closed)
        {
            UdpReceiveResult result;
            try { result = await udp.ReceiveAsync(); }
            catch (Exception) when (!closed) { continue; } // e.g. ICMP connection reset
            catch (Exception) { return; }

            var p = TryDecode(result.Buffer);
            if (p == null || p.Id == Id) continue;

            if (p.T == "bye")
            {
                peers.Remove(p.Id);
                continue;
            }
            bool isNew = !peers.ContainsKey(p.Id);
            Upsert(p, result.RemoteEndPoint.Address);
            if (isNew && p.T == "hello")
            {
                // Let the newcomer know about us right away instead of waiting for our next broadcast.
                var bytes = Encode("here");
                try { udp.Send(bytes, bytes.Length, result.RemoteEndPoint); } catch (SocketException) { }
            }
        }
    }

    async Task AcceptLoop()
    {
        while (!closed)
        {
            TcpClient client;
            try { client = await tcp.AcceptTcpClientAsync(); }
            catch (Exception) when (!closed) { continue; }
            catch (Exception) { return; }
            _ = HandleClient(client);
        }
    }

    async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            try
            {
                var remote = ((IPEndPoint)client.Client.RemoteEndPoint!).Address;
                using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                var line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
                var p = line == null ? null : TryDecode(Encoding.UTF8.GetBytes(line));
                if (p is not { T: "msg", Text: not null } || closed) return;
                MessageReceived?.Invoke(Upsert(p, remote), p.Text);
            }
            catch (Exception) { }
        }
    }

    static Packet? TryDecode(byte[] data)
    {
        try { return JsonSerializer.Deserialize<Packet>(data); }
        catch (JsonException) { return null; }
    }

    public void Dispose()
    {
        if (closed) return;
        Broadcast("bye");
        closed = true;
        timer.Dispose();
        udp.Dispose();
        tcp.Stop();
    }
}
