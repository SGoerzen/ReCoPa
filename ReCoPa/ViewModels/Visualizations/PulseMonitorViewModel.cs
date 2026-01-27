using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ReCoPa.ViewModels.Visualizations;

public sealed class PulseMonitorViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    public ObservableCollection<DateTimePoint> Values { get; } = new();

    public ISeries[] Series { get; }
    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }

    private DateTime _startTime;

    private bool _isReading = true;
    public bool IsReading
    {
        get => _isReading;
        set
        {
            if (SetProperty(ref _isReading, value))
            {
                if (_isReading) _timer.Start();
                else _timer.Stop();
            }
        }
    }

    public PulseMonitorViewModel()
    {
        _startTime = DateTime.Now;
        // Series
        Series = new ISeries[]
        {
            new LineSeries<DateTimePoint>
            {
                Values = Values,
                Fill = null,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 4)
            }
        };

        // Axes
        var x = new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.Black),
            SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 1),
            TextSize = 12,
            UnitWidth = TimeSpan.FromSeconds(1).Ticks,
            Labeler = v => FormatTime((long)v),

            // !!! hier NICHT über XAML binden – wir setzen CustomSeparators in Tick()
            CustomSeparators = GetSeparators()
        };

        var y = new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.Black),
            SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 1),
            TextSize = 12,
            MinLimit = 0,
            MaxLimit = 10
        };

        XAxes = new[] { x };
        YAxes = new[] { y };

        // UI-thread timer (kein lock nötig)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    private void Tick()
    {
        if (!IsReading) return;

        Values.Add(new DateTimePoint(DateTime.Now, _random.Next(0, 10)));
        if (Values.Count > 250) Values.RemoveAt(0);

        // !!! critical fix: CustomSeparators per Code setzen
        XAxes[0].CustomSeparators = GetSeparators();
    }
    

    private static double[] GetSeparators()
    {
        var now = DateTime.Now;
        return new double[]
        {
            now.AddSeconds(-25).Ticks,
            now.AddSeconds(-20).Ticks,
            now.AddSeconds(-15).Ticks,
            now.AddSeconds(-10).Ticks,
            now.AddSeconds(-5).Ticks,
            now.Ticks
        };
    }

    private static string FormatTime(long ticks)
    {
        var date = new DateTime(ticks);
        var secsAgo = (DateTime.Now - date).TotalSeconds;
        return secsAgo < 1 ? "now" : $"{secsAgo:N0}s ago";
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= (_, _) => Tick();
    }
}