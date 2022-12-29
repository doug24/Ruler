﻿using System.Windows;
using System.Windows.Input;

namespace Ruler
{
    /// <summary>
    /// Interaction logic for ShadowWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public event KeyEventHandler? ForwardKeyDown;
 
        public OverlayWindow(RulerViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            //PreviewKeyDown += Window_PreviewKeyDown;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            ForwardKeyDown?.Invoke(this, e);
        }

        //private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.A)
        //    {
        //        Close();
        //        e.Handled = true;
        //    }
        //}
    }
}