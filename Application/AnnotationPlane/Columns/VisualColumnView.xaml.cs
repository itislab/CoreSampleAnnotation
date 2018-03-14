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

namespace CoreSampleAnnotation.AnnotationPlane.Columns
{

    public class ViewPortConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (ImageSource)value;
            return new Rect(0, 0, source.Width, source.Height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PolygonPointsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return null;
            double width = (double)values[0];
            double height = (double)values[1];
            List<Point> result = new List<Point>();
            result.Add(new Point(0.0, 0.0));
            result.Add(new Point(width, 0.0));
            result.Add(new Point(width, height));
            result.Add(new Point(0.0, height));
            return new PointCollection(result);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for VisualColumnView.xaml
    /// </summary>
    public partial class VisualColumnView : UserControl
    {
        public VisualColumnView()
        {
            InitializeComponent();
        }
    }
}
