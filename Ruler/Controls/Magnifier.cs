using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using static Ruler.NativeMethods;

namespace Ruler
{
    public class Magnifier : Control, IDisposable
    {
        static Magnifier()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Magnifier),
                new FrameworkPropertyMetadata(typeof(Magnifier)));
        }

        private readonly DispatcherTimer timer;
        private Window? hostWindow;
        private IntPtr magnifierHandle = IntPtr.Zero;

        public float Magnification
        {
            get { return (float)GetValue(MagnificationProperty); }
            set { SetValue(MagnificationProperty, value); }
        }

        public static readonly DependencyProperty MagnificationProperty =
            DependencyProperty.Register("Magnification", typeof(float), typeof(Magnifier),
            new FrameworkPropertyMetadata(2f,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnMagnificationChanged));

        private static void OnMagnificationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Magnifier magnifier && magnifier.MagInitialized)
            {
                magnifier.SetMagnificationTransform();
            }
        }

        public Magnifier()
        {
            timer = new(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            timer.Tick += Timer_Tick;

            Loaded += Magnifier_Loaded;
        }

        private void Magnifier_Loaded(object sender, RoutedEventArgs e)
        {
            hostWindow = Window.GetWindow(this);
            if (hostWindow != null)
            {
                hostWindow.SizeChanged += Window_SizeChanged;
                hostWindow.Closing += Window_Closing;

                MagInitialized = MagInitialize();
                if (MagInitialized)
                {
                    CreateMagnifier();
                    timer.Start();
                }
            }
        }

        public bool MagInitialized { get; private set; }

        public bool EnableRefresh
        {
            get { return timer.IsEnabled; }
            set { timer.IsEnabled = value; }
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            timer.Stop();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeMagnifier();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateMaginifier();
        }

        public virtual void UpdateMaginifier()
        {
            if (!MagInitialized || magnifierHandle == IntPtr.Zero)
                return;

            POINT mousePoint = new();
            GetCursorPos(ref mousePoint);

            var width = DipConverter.DipsToPixelsX(RenderSize.Width / Magnification);
            var height = DipConverter.DipsToPixelsY(RenderSize.Height / Magnification);

            RECT sourceRect = new()
            {
                Left = mousePoint.X - (int)(width / 2),
                Top = mousePoint.Y - (int)(height / 2),
                Right = mousePoint.X + (int)(width / 2),
                Bottom = mousePoint.Y + (int)(height / 2)
            };

            Screen screen = Screen.FromHandle(magnifierHandle);
            var bounds = screen.BoundsPix;
            bounds.Inflate(width / 2, height / 2);

            // The center of the source rect must be able to reach
            // the edges of the screen to view the edge pixels.
            // But what happens when crossing to another screen?

            if (sourceRect.Left < bounds.Left)
            {
                sourceRect.Left = (int)bounds.Left;
            }
            if (sourceRect.Left > bounds.Right - width)
            {
                sourceRect.Left = (int)(bounds.Right - width);
            }
            sourceRect.Right = sourceRect.Left + (int)width;

            if (sourceRect.Top < bounds.Top)
            {
                sourceRect.Top = (int)bounds.Top;
            }
            if (sourceRect.Top > bounds.Height - height)
            {
                sourceRect.Top = (int)(bounds.Height - height);
            }
            sourceRect.Bottom = sourceRect.Top + (int)height;

            // Set the source rectangle for the magnifier control.
            MagSetWindowSource(magnifierHandle, sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            IntPtr hWndHost = new WindowInteropHelper(hostWindow).Handle;
            SetWindowPos(hWndHost, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE);

            // Force redraw.
            InvalidateRect(magnifierHandle, IntPtr.Zero, true);
        }

        private void ResizeMagnifier()
        {
            if (!MagInitialized || magnifierHandle == IntPtr.Zero)
                return;

            Window parent = Window.GetWindow(this);
            // get the magnifier bounds relative to the parent window
            Rect bounds = TransformToVisual(parent).TransformBounds(new Rect(RenderSize));
            // dips to pixels
            bounds.Transform(PresentationSource.FromVisual(parent).CompositionTarget.TransformToDevice);

            SetWindowPos(magnifierHandle, IntPtr.Zero,
                (int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height, 0);
        }

        private void CreateMagnifier()
        {
            if (!MagInitialized)
                return;

            IntPtr hInst = GetModuleHandle(null);
            IntPtr hWndHost = new WindowInteropHelper(hostWindow).Handle;

            Window parent = Window.GetWindow(this);
            // get the magnifier bounds relative to the parent window
            Rect bounds = TransformToVisual(parent).TransformBounds(new Rect(RenderSize));
            // dips to pixels
            bounds.Transform(PresentationSource.FromVisual(parent).CompositionTarget.TransformToDevice);

            // Create a magnifier control that fits the client area
            magnifierHandle = CreateWindow(0, "Magnifier", "MagnifierWindow",
                //WS_CHILD | WS_VISIBLE, (int)Margin.Left + 1, (int)Margin.Top + 1, width, height,
                WS_CHILD | WS_VISIBLE, (int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height,
                hWndHost, IntPtr.Zero, hInst, IntPtr.Zero);

            if (magnifierHandle == IntPtr.Zero)
            {
                return;
            }

            SetMagnificationTransform();
        }

        private void SetMagnificationTransform()
        {
            // Set the magnification factor.
            Transformation matrix = new(Magnification);
            MagSetWindowTransform(magnifierHandle, ref matrix);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Magnifier()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            timer.IsEnabled = false;
            if (hostWindow != null)
            {
                hostWindow.SizeChanged -= Window_SizeChanged;
            }
            if (MagInitialized)
            {
                MagUninitialize();
            }
        }

        #endregion
    }
}
