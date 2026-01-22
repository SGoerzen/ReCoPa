using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReCoPa.Network;

// Server-side event router
internal sealed class EventRouter
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<Func<SocketEventContext, Task>>> _handlers =
        new(StringComparer.Ordinal);

    public void On(string eventName, Func<SocketEventContext, Task> handler)
    {
        lock (_gate)
        {
            if (!_handlers.TryGetValue(eventName, out var list))
            {
                list = new List<Func<SocketEventContext, Task>>();
                _handlers[eventName] = list;
            }
            list.Add(handler);
        }
    }

    public void On(string eventName, Action<SocketEventContext> handler)
        => On(eventName, ctx => { handler(ctx); return Task.CompletedTask; });

    public async Task DispatchAsync(SocketEventContext ctx, Action<Exception>? onError)
    {
        List<Func<SocketEventContext, Task>>? list;
        lock (_gate)
        {
            _handlers.TryGetValue(ctx.EventName, out list);
            list = list == null ? null : new List<Func<SocketEventContext, Task>>(list);
        }

        if (list == null) return;

        foreach (var h in list)
        {
            try { await h(ctx).ConfigureAwait(false); }
            catch (Exception ex) { onError?.Invoke(ex); }
        }
    }
}