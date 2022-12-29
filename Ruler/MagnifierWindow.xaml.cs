using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static Ruler.NativeMethods;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for MagnifierWindow.xaml
    /// </summary>
    public partial class MagnifierWindow : Window
    {
        public event KeyEventHandler? ForwardKeyDown;

        private readonly Point Empty = new(double.MinValue, double.MinValue);
        private bool isRunning = false;
        private Point lastSnapPos = new(double.MinValue, double.MinValue);

        public MagnifierWindow(RulerViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            Closing += (s, e) => magControl?.Dispose();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            ForwardKeyDown?.Invoke(this, e);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                handled = true;
                // In a window currently covered by another window in the same thread
                // (the message will be sent to underlying windows in the same thread
                // until one of them returns a code that is not HTTRANSPARENT).
                return HTTRANSPARENT;
            }

            return IntPtr.Zero;
        }

        public void MoveTo(Point pt)
        {
            if (!IsRunning)
                return;

            if (lastSnapPos == Empty ||
                Math.Abs(lastSnapPos.X - pt.X) > 1 ||
                Math.Abs(lastSnapPos.Y - pt.Y) > 1)
            {
                double offsetX = ActualWidth / 2;
                double offsetY = ActualHeight / 2;

                Left = pt.X - offsetX;
                Top = pt.Y - offsetY;

                magControl?.UpdateMaginifier();
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                if (value == isRunning)
                    return;

                isRunning = value;
                if (magControl != null)
                {
                    magControl.EnableRefresh = isRunning;
                }
            }
        }
    }
}
