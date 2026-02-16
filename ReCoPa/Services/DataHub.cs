using System;
using System.Collections.Generic;
using System.Linq;
using ReCoPa.Plugins;

namespace ReCoPa.Services;

public sealed class DataHub : IDataAccess, IDataStore
{
    private readonly List<DataPacket> _data = new();
    private readonly object _gate = new();
    private readonly int _maxEntries;

    private event Action<DataPacket>? DataReceived;
    private readonly List<IDisposable> _pluginSubscriptions = new();
    private readonly object _pluginGate = new();

    public DataHub(int maxEntries = 10_000)
    {
        _maxEntries = Math.Max(1, maxEntries);
    }

    public IDataStore Store => this;

    public void Publish(string eventName, string payload)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        var packet = new DataPacket(eventName, payload ?? string.Empty, DateTime.UtcNow);

        lock (_gate)
        {
            _data.Add(packet);
            var overflow = _data.Count - _maxEntries;
            if (overflow > 0)
                _data.RemoveRange(0, overflow);
        }

        var handlers = DataReceived;
        if (handlers == null)
            return;

        foreach (Action<DataPacket> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(packet);
            }
            catch
            {
                // Ignore plugin handler errors to keep the data pipeline alive.
            }
        }
    }

    public IDisposable Subscribe(Action<DataPacket> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        DataReceived += handler;
        return new Subscription(() => DataReceived -= handler);
    }

    public IReadOnlyList<DataPacket> Query(DataQuery query)
    {
        if (query == null)
            query = new DataQuery();

        List<DataPacket> snapshot;
        lock (_gate)
            snapshot = new List<DataPacket>(_data);

        IEnumerable<DataPacket> filtered = snapshot;
        if (!string.IsNullOrWhiteSpace(query.EventName))
            filtered = filtered.Where(p => string.Equals(p.EventName, query.EventName, StringComparison.Ordinal));
        if (query.SinceUtc.HasValue)
            filtered = filtered.Where(p => p.TimestampUtc >= query.SinceUtc.Value);
        if (query.UntilUtc.HasValue)
            filtered = filtered.Where(p => p.TimestampUtc <= query.UntilUtc.Value);

        var list = query.NewestFirst
            ? filtered.OrderByDescending(p => p.TimestampUtc).ToList()
            : filtered.OrderBy(p => p.TimestampUtc).ToList();

        if (query.Limit is > 0 && list.Count > query.Limit.Value)
            list = list.Take(query.Limit.Value).ToList();

        return list;
    }

    public DataPacket? Latest(string? eventName = null)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return _data.Count == 0 ? null : _data[^1];

            for (var i = _data.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_data[i].EventName, eventName, StringComparison.Ordinal))
                    return _data[i];
            }

            return null;
        }
    }

    public int Count(string? eventName = null)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                return _data.Count;

            var count = 0;
            foreach (var item in _data)
            {
                if (string.Equals(item.EventName, eventName, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }
    }

    public void BindPluginComponents(IEnumerable<IPluginComponent> components)
    {
        if (components == null)
            return;

        lock (_pluginGate)
        {
            foreach (var sub in _pluginSubscriptions)
                sub.Dispose();
            _pluginSubscriptions.Clear();
        }

        foreach (var component in components)
        {
            if (component is IDataAccessConsumer accessConsumer)
                accessConsumer.SetDataAccess(this);

            if (component is IDataConsumer consumer)
            {
                var sub = Subscribe(consumer.OnData);
                lock (_pluginGate) _pluginSubscriptions.Add(sub);
            }
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Action _dispose;
        private int _disposed;

        public Subscription(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            _dispose();
        }
    }
}
