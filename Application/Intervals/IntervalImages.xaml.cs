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

namespace CoreSampleAnnotation.Intervals
{
    /// <summary>
    /// Interaction logic for IntervalImages.xaml
    /// </summary>
    public partial class IntervalImages : UserControl
    {
        public IntervalImages()
        {
            InitializeComponent();
        }
    }

    public class FileExistsVisibilityconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (str != null)
            {
                if (System.IO.File.Exists(str))
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;

            }
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MoreThanOneVisibilityConverter : IValueConverter {
        
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? v = value as int?;
            if (v != null && v.HasValue)
            {
                if (v.Value > 1)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
