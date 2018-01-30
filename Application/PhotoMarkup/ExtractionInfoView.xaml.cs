using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoreSampleAnnotation.PhotoMarkup
{
    /// <summary>
    /// Interaction logic for ExtractionInfoView.xaml
    /// </summary>
    public partial class ExtractionInfoView : UserControl
    {
        public ExtractionInfoView()
        {
            InitializeComponent();

            Button.MouseDown += Button_MouseDown;
            Button.MouseUp += Button_MouseUp;
            Button.MouseLeave += Button_MouseLeave;

            Button.TouchDown += Button_TouchDown;
            Button.TouchUp += Button_TouchUp;
            Button.TouchLeave += Button_TouchLeave;

            HoverWindow.Visibility = Visibility.Collapsed;
        }

        private void Button_TouchLeave(object sender, TouchEventArgs e)
        {
            HoverWindow.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void Button_TouchUp(object sender, TouchEventArgs e)
        {
            HoverWindow.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void Button_TouchDown(object sender, TouchEventArgs e)
        {
            HoverWindow.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverWindow.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HoverWindow.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HoverWindow.Visibility = Visibility.Visible;
            e.Handled = true;
        }
    }
}
