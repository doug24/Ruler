using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static Ruler.NativeMethods;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RulerViewModel viewModel = new();
        private readonly DispatcherTimer dispatcherTimer;
        private SizeToolsWindow? dlg;
        private OverlayWindow? overlay;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel;

            Top = 400;
            Left = 200;

            dispatcherTimer = new(TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Render, TimerCallback, Dispatcher);

            StateChanged += (s, e) => WindowState = WindowState.Normal;
            Loaded += (s, e) => dispatcherTimer.Start();
            SizeChanged += (s, e) => InvalidateVisual();
            LocationChanged += MainWindow_LocationChanged;

            SourceInitialized += MainWindow_SourceInitialized;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            ruler.MouseDown += MainWindow_MouseDown;
            ruler.PreviewMouseDown += Ruler_PreviewMouseDown;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.Flip) ||
                e.PropertyName == nameof(viewModel.ZeroPoint) ||
                e.PropertyName == nameof(viewModel.RulerStyle))
            {
                UpdateOrigin();
            }
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            UpdateOrigin();
            InvalidateVisual();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            if (e.Key == Key.Right)
            {
                Left += ctrl ? 5 : 0.5;
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                Left -= ctrl ? 5 : 0.5;
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                Top -= ctrl ? 5 : 0.5;
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                Top += ctrl ? 5 : 0.5;
                e.Handled = true;
            }
            else if (e.Key == Key.A)
            {
                if (overlay != null && overlay.Visibility == Visibility.Visible)
                {
                    overlay.Close();
                }
                else
                {
                    ShowAngle_Click(this, new());
                }
            }
            else if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }
        private void TimerCallback(object? sender, EventArgs e)
        {
            if (overlay != null)
            {
                viewModel.MousePoint = MouseTracker.GetMousePosition(overlay);
            }

            var newPosition = MouseTracker.GetTrackPoint(this, viewModel.RulerStyle, viewModel.ActiveEdge);
            if (newPosition != viewModel.TrackPoint)
            {
                viewModel.TrackPoint = newPosition;
            }
        }

        private void Ruler_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDoubleClick(e))
            {
                ShowLayoutDialog_Click(sender, e);
                e.Handled = true;
            }
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private readonly Stopwatch doubleClickStopwatch = new();
        private Point lastClickLocation;
        private bool IsDoubleClick(MouseButtonEventArgs e)
        {
            Point currentLocation = e.GetPosition(this);
            double dist = Math.Abs(Point.Subtract(currentLocation, lastClickLocation).Length);
            bool clicksAreCloseInDistance = dist < 40;
            lastClickLocation = currentLocation;

            TimeSpan elapsed = doubleClickStopwatch.Elapsed;
            doubleClickStopwatch.Restart();
            bool clicksAreCloseInTime = (elapsed != TimeSpan.Zero && elapsed < TimeSpan.FromSeconds(0.7));

            if (!clicksAreCloseInTime)
            {
                lastClickLocation = new(0, 0);
            }

            return clicksAreCloseInDistance && clicksAreCloseInTime;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            overlay?.Close();
            dlg?.Close();
            base.OnClosing(e);
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                var value = GetWindowLong(hwnd, GWL_STYLE);
                _ = SetWindowLong(hwnd, GWL_STYLE, value & ~WS_MAXIMIZEBOX);
            }
        }

        private void ShowLayoutDialog_Click(object sender, RoutedEventArgs e)
        {
            if (dlg == null)
            {
                dlg = new(viewModel);
                dlg.Closing += (s, e) => dlg = null;
            }
            dlg.Show();
        }

        private void ShowAngle_Click(object sender, RoutedEventArgs e)
        {
            if (overlay == null)
            {
                overlay = new(viewModel)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                overlay.Closing += (s, e) => overlay = null;
            }

            UpdateOrigin();

            Screen screen = Screen.FromWindow(this);
            overlay.Left = 0;
            overlay.Top = 0;
            overlay.Width = screen.WorkingAreaDip.Width;
            overlay.Height = screen.WorkingAreaDip.Height;
            overlay.Show();

        }

        private void UpdateOrigin()
        {
            double x, y;

            if (viewModel.RulerStyle == RulerStyle.Horizontal)
            {
                x = viewModel.ZeroPoint == ZeroPoint.Near ? Left : Left + Width;
                y = viewModel.Flip ? Top + Height : Top;
            }
            else
            {
                x = viewModel.Flip ? Left : Left + Width;
                y = viewModel.ZeroPoint == ZeroPoint.Far ? Top + Height : Top;
            }

            viewModel.Origin = new(x, y);
        }

    }
}
