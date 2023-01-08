using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
        private OptionsWindow? optionsDialog;
        private OverlayWindow? angleWindow;
        private MagnifierWindow? magnifierWindow;
        private double minDip = 1d;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel;

            dispatcherTimer = new(TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Render, TimerCallback, Dispatcher);

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            LocationChanged += MainWindow_LocationChanged;

            SourceInitialized += MainWindow_SourceInitialized;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            ruler.MouseDown += MainWindow_MouseDown;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            Screen scr = Screen.FromWindow(this);
            minDip = 1.0 / scr.ScaleX;

            dispatcherTimer.Start();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is (nameof(viewModel.Flip)) or
                (nameof(viewModel.ZeroPoint)) or
                (nameof(viewModel.Orientation)))
            {
                UpdateOrigin();
            }
            else if (e.PropertyName is nameof(viewModel.AngleVisible))
            {
                ShowAngle(viewModel.AngleVisible);
            }
            else if (e.PropertyName is nameof(viewModel.MagnifierVisible))
            {
                ShowMagnifier(viewModel.MagnifierVisible);
            }
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            Screen scr = Screen.FromWindow(this);
            minDip = 1.0 / scr.ScaleX;

            UpdateOrigin();
            UpdateTooltip();
        }

        private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            var screen = Screen.FromWindow(this);

            // does it look like the ruler is docked to the right?
            if (e.WidthChanged &&
                Top == 0 && ActualHeight == screen.WorkingAreaDip.Height &&
                Left == screen.WorkingAreaDip.Width - e.PreviousSize.Width)
            {
                // adjust position for the new width
                Left = screen.WorkingAreaDip.Right - e.NewSize.Width;
            }

            // does it look like the ruler is docked to the bottom?
            if (e.HeightChanged &&
                Left == 0 && ActualWidth == screen.WorkingAreaDip.Width &&
                Top == screen.WorkingAreaDip.Height - e.PreviousSize.Height)
            {
                // adjust position for the new height
                Top = screen.WorkingAreaDip.Height - e.NewSize.Height;
            }
        }

        private void UpdateTooltip()
        {
            viewModel.ScaleToolTip = $"Left: {Left}" + Environment.NewLine +
                $"Top: {Top}";
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            bool alt = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
            bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            double dipOffset = shift ? 10 * minDip : minDip;
            int pixelOffset = shift ? 10 : 1;

            switch (key)
            {
                case Key.Right:
                    if (ctrl)
                    {
                        MoveCursor(pixelOffset, 0);
                        e.Handled = true;
                    }
                    else if (alt)
                    {
                        if (viewModel.Orientation == Orientation.Horizontal)
                        {
                            Width += dipOffset;
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        Left += dipOffset;
                        e.Handled = true;
                    }
                    break;
                case Key.Left:
                    if (ctrl)
                    {
                        MoveCursor(-pixelOffset, 0);
                        e.Handled = true;
                    }
                    else if (alt)
                    {
                        if (viewModel.Orientation == Orientation.Horizontal)
                        {
                            Width -= dipOffset;
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        Left -= dipOffset;
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (ctrl)
                    {
                        MoveCursor(0, -pixelOffset);
                        e.Handled = true;
                    }
                    else if (alt)
                    {
                        if (viewModel.Orientation == Orientation.Vertical)
                        {
                            Height -= dipOffset;
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        Top -= dipOffset;
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (ctrl)
                    {
                        MoveCursor(0, pixelOffset);
                        e.Handled = true;
                    }
                    else if (alt)
                    {
                        if (viewModel.Orientation == Orientation.Vertical)
                        {
                            Height += dipOffset;
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        Top += dipOffset;
                        e.Handled = true;
                    }
                    break;
                case Key.A:
                    viewModel.AngleVisible = !viewModel.AngleVisible;
                    e.Handled = true;
                    break;
                case Key.Q:
                    viewModel.MagnifierVisible = !viewModel.MagnifierVisible;
                    e.Handled = true;
                    break;
                case Key.H:
                    viewModel.Orientation = Orientation.Horizontal;
                    e.Handled = true;
                    break;
                case Key.V:
                    viewModel.Orientation = Orientation.Vertical;
                    e.Handled = true;
                    break;
                case Key.S:
                    viewModel.Flip = !viewModel.Flip;
                    e.Handled = true;
                    break;
                case Key.N:
                    viewModel.ThinScale = !viewModel.ThinScale;
                    e.Handled = true;
                    break;
                case Key.OemOpenBrackets:
                    viewModel.ZeroPoint = ZeroPoint.Near;
                    e.Handled = true;
                    break;
                case Key.OemCloseBrackets:
                    viewModel.ZeroPoint = ZeroPoint.Far;
                    e.Handled = true;
                    break;
                case Key.P:
                    viewModel.TopMost = !viewModel.TopMost;
                    e.Handled = true;
                    break;
                case Key.D1:
                    viewModel.ScaleUnits = Units.Pixel;
                    e.Handled = true;
                    break;
                case Key.D2:
                    viewModel.ScaleUnits = Units.DIP;
                    e.Handled = true;
                    break;
                case Key.D3:
                    viewModel.ScaleUnits = Units.Point;
                    e.Handled = true;
                    break;
                case Key.D4:
                    viewModel.ScaleUnits = Units.CM;
                    e.Handled = true;
                    break;
                case Key.D5:
                    viewModel.ScaleUnits = Units.Inch;
                    e.Handled = true;
                    break;
                case Key.D6:
                    viewModel.ScaleUnits = Units.Percent;
                    e.Handled = true;
                    break;
                case Key.M:
                    viewModel.SetMarker();
                    e.Handled = true;
                    break;
                case Key.D:
                    viewModel.RemoveMarker();
                    e.Handled = true;
                    break;
                case Key.C:
                    viewModel.ClearMarkers();
                    e.Handled = true;
                    break;
                case Key.O:
                    ShowOptionsDialog_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.OemMinus:
                    WindowState = WindowState.Minimized;
                    e.Handled = true;
                    break;
                case Key.OemPlus:
                    viewModel.Maximize();
                    e.Handled = true;
                    break;
                case Key.Back:
                    viewModel.RestoreSnapshot();
                    e.Handled = true;
                    break;
                case Key.T:
                    viewModel.DockRuler(Placement.Top);
                    e.Handled = true;
                    break;
                case Key.B:
                    viewModel.DockRuler(Placement.Bottom);
                    e.Handled = true;
                    break;
                case Key.L:
                    viewModel.DockRuler(Placement.Left);
                    e.Handled = true;
                    break;
                case Key.R:
                    viewModel.DockRuler(Placement.Right);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    Exit_Click(sender, new());
                    e.Handled = true;
                    break;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private static void MoveCursor(int xOffset, int yOffset)
        {
            POINT mousePoint = new();
            if (GetCursorPos(ref mousePoint))
            {
                SetCursorPos(mousePoint.X + xOffset, mousePoint.Y + yOffset);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TimerCallback(object? sender, EventArgs e)
        {
            if (angleWindow != null)
            {
                viewModel.MousePoint = MouseTracker.GetMousePosition(angleWindow);
            }

            magnifierWindow?.MoveTo(MouseTracker.GetMousePosition(this));

            var newPosition = MouseTracker.GetTrackPoint(this, viewModel.Orientation, viewModel.ActiveEdge);
            if (newPosition != viewModel.TrackPoint)
            {
                viewModel.TrackPoint = newPosition;
            }
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                DragMove();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            viewModel.SaveSettings();

            angleWindow?.Close();
            magnifierWindow?.Close();
            optionsDialog?.Close();
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

        private void ShowOptionsDialog_Click(object sender, RoutedEventArgs e)
        {
            if (optionsDialog == null)
            {
                optionsDialog = new(viewModel);
                optionsDialog.Closing += (s, e) => optionsDialog = null;
            }
            optionsDialog.Show();
        }

        private void ShowAngle(bool angleVisible)
        {
            if (angleVisible)
            {
                if (angleWindow == null)
                {
                    angleWindow = new(viewModel)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.Manual
                    };
                    angleWindow.ForwardKeyDown += (s, k) => MainWindow_PreviewKeyDown(s, k);
                    angleWindow.Closing += (s, e) => angleWindow = null;
                }

                UpdateOrigin();

                Screen screen = Screen.FromWindow(this);
                angleWindow.Left = 0;
                angleWindow.Top = 0;
                angleWindow.Width = screen.WorkingAreaDip.Width;
                angleWindow.Height = screen.WorkingAreaDip.Height;
                angleWindow.Show();
            }
            else
            {
                angleWindow?.Close();
            }
        }

        private void UpdateOrigin()
        {
            double x, y;

            if (viewModel.Orientation == Orientation.Horizontal)
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

        private void ShowMagnifier(bool magnifierVisible)
        {
            if (magnifierWindow == null)
            {
                magnifierWindow = new(viewModel)
                {
                    Owner = this
                };
                magnifierWindow.ForwardKeyDown += (s, k) => MainWindow_PreviewKeyDown(s, k);
            }

            if (magnifierVisible)
            {
                magnifierWindow.IsRunning = true;
                magnifierWindow.Show();
                magnifierWindow.MoveTo(MouseTracker.GetMousePosition(this));
            }
            else if (magnifierWindow != null)
            {
                magnifierWindow.IsRunning = false;
                magnifierWindow.Visibility = Visibility.Hidden;
            }
        }
    }
}
