using System.Windows;
using System.Windows.Input;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for ShadowWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public event KeyEventHandler? ForwardKeyDown;

        public OverlayWindow(RulerViewModel vm, Screen screen)
        {
            InitializeComponent();
            DataContext = vm;
            DisplayName = screen.DeviceName;

            Left = screen.WorkingAreaDip.Left;
            Top = screen.WorkingAreaDip.Top;
            Width = screen.WorkingAreaDip.Width;
            Height = screen.WorkingAreaDip.Height;
        }

        public string DisplayName { get; private set; }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            ForwardKeyDown?.Invoke(this, e);
        }
    }
}
