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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel;

            dispatcherTimer = new(TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Render, TimerCallback, Dispatcher);

            StateChanged += (s, e) => WindowState = WindowState.Normal;
            Loaded += (s, e) => dispatcherTimer.Start();
            LocationChanged += (s, e) => InvalidateVisual();
            SizeChanged += (s, e) => InvalidateVisual();

            SourceInitialized += MainWindow_SourceInitialized;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            ruler.MouseDown += MainWindow_MouseDown;
            ruler.PreviewMouseDown += Ruler_PreviewMouseDown;
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
            else if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void TimerCallback(object? sender, EventArgs e)
        {
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

    }
}
