using System.Windows;
using System.Windows.Input;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow(RulerViewModel vm)
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
