using System.Windows;
using System.Windows.Input;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for ShadowWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public OverlayWindow(RulerViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}
