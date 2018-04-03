using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class ImageCanvasTopConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 5)
                return null;
            double i_up_d = (double)values[0];
            double i_lo_d = (double)values[1];
            double col_up_d = (double)values[2];
            double col_lo_d = (double)values[3];
            double col_wpf_height = (double)values[4];
            double top = (i_up_d - col_up_d) / (col_lo_d - col_up_d) * col_wpf_height;
            //System.Diagnostics.Trace.WriteLine(string.Format("image top is {0}", top));
            return top;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageCanvasHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 5)
                return null;
            double i_up_d = (double)values[0];
            double i_lo_d = (double)values[1];
            double col_up_d = (double)values[2];
            double col_lo_d = (double)values[3];
            double col_wpf_height = (double)values[4];

            double height = (i_lo_d - i_up_d) / (col_lo_d - col_up_d) * col_wpf_height;
            //System.Diagnostics.Trace.WriteLine(string.Format("image height is {0}", height));
            return height;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    

    /// <summary>
    /// Interaction logic for ImageColumnView.xaml
    /// </summary>
    public partial class ImageColumnView : UserControl
    {
        //public event EventHandler<NewLayerBoundaryRequestedEventArgs> NewLayerBoundaryRequested = null;

        public ImageColumnView()
        {
            InitializeComponent();            
        }        
    }
}
