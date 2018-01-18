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

namespace All
{
    public class UpBotomSideToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values != null) && (values.Length == 3))
            {
                Point bottomCentre = (Point)values[0];
                Point upCentre = (Point)values[1];
                Point side = (Point)values[2];

                double y2y1 = upCentre.Y - bottomCentre.Y;
                double x2x1 = upCentre.X - bottomCentre.X;

                double doub_y2y1 = y2y1 * y2y1;
                double doub_x2x1 = x2x1 * x2x1;

                double Xt =
                    (bottomCentre.X * doub_y2y1 +
                    side.X * doub_x2x1 +
                    x2x1 * y2y1 * (side.Y - bottomCentre.Y)) /
                        (doub_y2y1 + doub_x2x1);
                double Yt =
                    x2x1 * (side.X - Xt) / y2y1 + side.Y;

                Point t = new Point(Xt, Yt);

                Vector toSide = side - t;
                Point upperLeft = upCentre - toSide;
                Point upperRight = upCentre + toSide;
                Point lowerLeft = bottomCentre - toSide;
                Point lowerRight = bottomCentre + toSide;
                var pointsCollection = new PointCollection(new Point[] { upperLeft, upperRight, lowerRight, lowerLeft });
                return pointsCollection;
            }
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for AnnotatedPolygon.xaml
    /// </summary>
    public partial class AnnotatedPolygon : UserControl
    {
        public AnnotatedPolygon()
        {
            InitializeComponent();
        }        
    }
}
