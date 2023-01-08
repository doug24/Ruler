using System.Windows;
using System.Windows.Input;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for SizeToolsWindow.xaml
    /// </summary>
    public partial class SizeToolsWindow : Window
    {
        public SizeToolsWindow(RulerViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }
    }
}
