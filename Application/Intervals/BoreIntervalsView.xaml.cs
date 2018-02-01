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
    /// Interaction logic for BoreIntervalsView.xaml
    /// </summary>
    public partial class BoreIntervalsView : UserControl
    {
        public BoreIntervalsView()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Uses default culture (not invariant culture!)
    /// </summary>
    public class NanToEmptyStringDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? d = value as double?;
            if (d.HasValue) {
                if (double.IsNaN(d.Value))
                    return "";
                else
                    return d.Value.ToString();
            }
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (str != null)
            {
                if (str == string.Empty)
                    return null;
                else
                {
                    double result;
                    if (double.TryParse(str, out result))
                        return result;
                    else
                        return null;
                }
            }
            else
                return null;
        }
    }    
}
