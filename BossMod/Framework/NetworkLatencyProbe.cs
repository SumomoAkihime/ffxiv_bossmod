using System.Net;
using System.Threading.Tasks;

namespace BossMod;

// Same Windows RTT source as upstream PingService; the existing plugin owns
// this probe, so no hosted-service or additional package is required.
sealed partial class NetworkLatencyProbe : IDisposable
{
    private string _host = string.Empty;
    private Task<float?>? _pending;
    private string _pendingHost = string.Empty;
    private DateTime _nextPoll;
    private bool _disposed;

    public float? Update(string host, DateTime now)
    {
        if (_disposed)
            return null;
        float? sample = null;
        if (_host != host)
        {
            _host = host;
            _nextPoll = default;
            sample = 0; // Never apply an old server's sample to a new connection.
        }
        if (_pending is { IsCompleted: true })
        {
            if (_pendingHost == host && _pending.Status == TaskStatus.RanToCompletion)
                sample = _pending.Result ?? sample;
            _pending = null;
        }
        if (_pending == null && now >= _nextPoll)
        {
            _nextPoll = now.AddSeconds(3);
            if (IPAddress.TryParse(host, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
            {
                _pendingHost = host;
                var raw = BitConverter.ToUInt32(address.GetAddressBytes());
                _pending = Task.Run<float?>(() =>
                {
                    var hops = 0u;
                    var rtt = 0u;
                    return GetRTTAndHopCount(raw, ref hops, 51, ref rtt) == 1 ? rtt * 0.001f : null;
                });
            }
        }
        return sample;
    }

    // The native call can block. It owns only value data and is never awaited
    // from the game thread or allowed to publish after disposal.
    public void Dispose() => _disposed = true;

    [LibraryImport("Iphlpapi.dll", EntryPoint = "GetRTTAndHopCount", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetRTTAndHopCount(uint address, ref uint hopCount, uint maxHops, ref uint rtt);
}
