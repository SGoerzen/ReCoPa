using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ReCoPa.Views;

public partial class TabsHeaderView : UserControl, INotifyPropertyChanged
{
    private ScrollViewer? _tabsScroll;

    private bool _panActivated;

    
    private bool _isPanning;
    private Point _lastPoint;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<TabsHeaderView, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<ICommand?> SelectTabCommandProperty =
        AvaloniaProperty.Register<TabsHeaderView, ICommand?>(nameof(SelectTabCommand));

    public static readonly StyledProperty<ICommand?> AddClientCommandProperty =
        AvaloniaProperty.Register<TabsHeaderView, ICommand?>(nameof(AddClientCommand));

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public ICommand? SelectTabCommand
    {
        get => GetValue(SelectTabCommandProperty);
        set => SetValue(SelectTabCommandProperty, value);
    }

    public ICommand? AddClientCommand
    {
        get => GetValue(AddClientCommandProperty);
        set => SetValue(AddClientCommandProperty, value);
    }

    public bool CanScrollLeft
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanScrollRight
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public TabsHeaderView()
    {
        InitializeComponent();

        _tabsScroll = this.FindControl<ScrollViewer>("TabsScroll");

        if (_tabsScroll != null)
        {
            _tabsScroll.ScrollChanged += (_, __) => UpdateArrowState();
            _tabsScroll.GetObservable(BoundsProperty).Subscribe(_ => UpdateArrowState());
            UpdateArrowState();
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void ScrollTabsLeft(object? sender, RoutedEventArgs e)
    {
        if (_tabsScroll is null) return;
        const double step = 220;
        var x = Math.Max(0, _tabsScroll.Offset.X - step);
        _tabsScroll.Offset = new Vector(x, _tabsScroll.Offset.Y);
        UpdateArrowState();
    }

    private void ScrollTabsRight(object? sender, RoutedEventArgs e)
    {
        if (_tabsScroll is null) return;
        const double step = 220;
        var x = _tabsScroll.Offset.X + step;
        _tabsScroll.Offset = new Vector(x, _tabsScroll.Offset.Y);
        UpdateArrowState();
    }

    private void UpdateArrowState()
    {
        if (_tabsScroll is null) { CanScrollLeft = false; CanScrollRight = false; return; }

        var offsetX = _tabsScroll.Offset.X;
        var viewportW = _tabsScroll.Viewport.Width;
        var extentW = _tabsScroll.Extent.Width;

        // small epsilon avoids flicker due to rounding
        const double eps = 0.5;

        CanScrollLeft = offsetX > eps;
        CanScrollRight = offsetX + viewportW < extentW - eps;
    }

    private void TabsScroll_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_tabsScroll is null) return;

        _isPanning = true;
        _panActivated = false;
        _lastPoint = e.GetPosition(_tabsScroll);

        // NICHT sofort capturen, sonst frisst du Tab-Klicks.
        // Capture erst, wenn wirklich "drag" erkannt wurde.
    }

    private void TabsScroll_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_tabsScroll is null || !_isPanning) return;

        var p = e.GetPosition(_tabsScroll);
        var dx = p.X - _lastPoint.X;

        // erst ab kleiner Schwelle als "pan" werten
        if (!_panActivated)
        {
            if (Math.Abs(dx) < 4) return;

            _panActivated = true;

            // ✅ Avalonia Pointer Capture
            e.Pointer.Capture(_tabsScroll);

            // ab jetzt sollen Clicks nicht mehr durchgehen
            e.Handled = true;
        }

        // scrollen
        var target = _tabsScroll.Offset.X - dx;

        var maxX = Math.Max(0, _tabsScroll.Extent.Width - _tabsScroll.Viewport.Width);
        if (target < 0) target = 0;
        if (target > maxX) target = maxX;

        _tabsScroll.Offset = _tabsScroll.Offset.WithX(target);

        _lastPoint = p;
        e.Handled = true;

        UpdateArrowState();
    }

    private void TabsScroll_PointerReleased(object? sender, PointerReleasedEventArgs e)
        => EndPan(e.Pointer);

    private void TabsScroll_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        => EndPan(e.Pointer);

    private void EndPan(IPointer pointer)
    {
        _isPanning = false;
        _panActivated = false;

        // ✅ release capture
        pointer.Capture(null);

        UpdateArrowState();
    }
    
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}