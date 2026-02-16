using System.Collections.Generic;
using ReCoPa.Plugins;
using ReCoPa.xAPI.ViewModels;
using ReCoPa.xAPI.Views;

namespace ReCoPa.xAPI.Plugins;

public sealed class ActivityPulse : IVisualization, IDataConsumer, IDataAccessConsumer
{
    public string Name => "Activity Pulse";

    private readonly Queue<DataPacket> _pending = new();
    private readonly object _gate = new();
    private const int PendingLimit = 200;

    private IDataAccess? _access;
    private ActivityPulseViewModel? _viewModel;

    public object CreateView()
    {
        var vm = new ActivityPulseViewModel();
        _viewModel = vm;

        if (_access != null)
            vm.SetDataAccess(_access);

        FlushPending(vm);
        return new ActivityPulseView(vm);
    }

    public void SetDataAccess(IDataAccess access)
    {
        _access = access;
        _viewModel?.SetDataAccess(access);
    }

    public void OnData(DataPacket data)
    {
        var vm = _viewModel;
        if (vm != null)
        {
            vm.OnData(data);
            return;
        }

        lock (_gate)
        {
            _pending.Enqueue(data);
            while (_pending.Count > PendingLimit)
                _pending.Dequeue();
        }
    }

    private void FlushPending(ActivityPulseViewModel vm)
    {
        DataPacket[] buffered;
        lock (_gate)
        {
            buffered = _pending.ToArray();
            _pending.Clear();
        }

        foreach (var item in buffered)
            vm.OnData(item);
    }
}
