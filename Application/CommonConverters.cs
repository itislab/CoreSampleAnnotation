using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CoreSampleAnnotation
{
    /// <summary>
    /// true => visible
    /// false => Collapsed
    /// </summary>
    public class CollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? b = value as bool?;
            if (b.HasValue)
            {
                if (b.Value)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// true => visible
    /// false => Collapsed
    /// </summary>
    public class VisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? b = value as bool?;
            if (b.HasValue)
            {
                if (b.Value)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
            if (d.HasValue)
            {
                if (double.IsNaN(d.Value))
                    return "";
                else
                    return d.Value.ToString((string)parameter);
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
