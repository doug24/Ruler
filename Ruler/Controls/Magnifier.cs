using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Magnification;
using Windows.Win32.UI.WindowsAndMessaging;

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

                MagInitialized = PInvoke.MagInitialize();
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

        unsafe public virtual void UpdateMaginifier()
        {
            if (!MagInitialized || magnifierHandle == IntPtr.Zero)
                return;

            PInvoke.GetCursorPos(out var mousePoint);

            var width = DipConverter.DipsToPixelsX(RenderSize.Width / Magnification);
            var height = DipConverter.DipsToPixelsY(RenderSize.Height / Magnification);

            RECT sourceRect = new()
            {
                left = mousePoint.X - (int)(width / 2),
                top = mousePoint.Y - (int)(height / 2),
                right = mousePoint.X + (int)(width / 2),
                bottom = mousePoint.Y + (int)(height / 2)
            };

            Screen screen = Screen.FromHandle(magnifierHandle);
            var bounds = screen.BoundsPix;
            bounds.Inflate(width / 2, height / 2);

            // The center of the source rect must be able to reach
            // the edges of the screen to view the edge pixels.
            // But what happens when crossing to another screen?

            if (sourceRect.left < bounds.Left)
            {
                sourceRect.left = (int)bounds.Left;
            }
            if (sourceRect.left > bounds.Right - width)
            {
                sourceRect.left = (int)(bounds.Right - width);
            }
            sourceRect.right = sourceRect.left + (int)width;

            if (sourceRect.top < bounds.Top)
            {
                sourceRect.top = (int)bounds.Top;
            }
            if (sourceRect.top > bounds.Height - height)
            {
                sourceRect.top = (int)(bounds.Height - height);
            }
            sourceRect.bottom = sourceRect.top + (int)height;

            // Set the source rectangle for the magnifier control.
            //MagSetWindowSource(magnifierHandle, sourceRect);
            PInvoke.MagSetWindowSource(new(magnifierHandle), sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            HWND hWnd = new(new WindowInteropHelper(hostWindow).Handle);
            PInvoke.SetWindowPos(hWnd, HWND.HWND_TOPMOST, 0, 0, 0, 0,
                SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);

            // Force redraw.
            PInvoke.InvalidateRect(new(magnifierHandle), bErase: true);
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

            PInvoke.SetWindowPos(new(magnifierHandle), new(IntPtr.Zero),
                (int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height, 0);
        }

        unsafe private void CreateMagnifier()
        {
            if (!MagInitialized)
                return;

            var hInst = PInvoke.GetModuleHandle(string.Empty);
            var hMenu = new FreeLibrarySafeHandle();
            var hWndHost = new WindowInteropHelper(hostWindow).Handle;

            Window parent = Window.GetWindow(this);
            // get the magnifier bounds relative to the parent window
            Rect bounds = TransformToVisual(parent).TransformBounds(new Rect(RenderSize));
            // dips to pixels
            bounds.Transform(PresentationSource.FromVisual(parent).CompositionTarget.TransformToDevice);

            // Create a magnifier control that fits the client area
            magnifierHandle = PInvoke.CreateWindowEx(0, "Magnifier", "MagnifierWindow",
                //WS_CHILD | WS_VISIBLE, (int)Margin.Left + 1, (int)Margin.Top + 1, width, height,
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE, (int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height,
                new(hWndHost), hMenu, hInst, null);

            if (magnifierHandle == IntPtr.Zero)
            {
                return;
            }

            SetMagnificationTransform();
        }

        unsafe private void SetMagnificationTransform()
        {
            // Set the magnification factor.
            var n = Magnification;
            // Set the magnification factor.
            MAGTRANSFORM matrix = new();
            matrix.v.Value[0] = n;
            matrix.v.Value[4] = n;
            matrix.v.Value[8] = 1f;

            PInvoke.MagSetWindowTransform(new(magnifierHandle), ref matrix);

            //Transformation matrix = new(Magnification);
            //MagSetWindowTransform(magnifierHandle, ref matrix);
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
                PInvoke.MagUninitialize();
            }
        }

        #endregion
    }
}
