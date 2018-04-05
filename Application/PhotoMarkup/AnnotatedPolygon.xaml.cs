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

namespace CoreSampleAnnotation.PhotoMarkup
{
    public static class Calc {
        public static Point FindNormalIntersection(Point bottomCentre, Point upCentre, Point side) {            
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

            return new Point(Xt, Yt);
        }
    }

    public class UpBottomSideToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values != null) && (values.Length == 3))
            {
                Point bottomCentre = (Point)values[0];
                Point upCentre = (Point)values[1];
                Point side = (Point)values[2];

                var t = Calc.FindNormalIntersection(bottomCentre, upCentre, side);

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

    public class LabelTransformConverter : IMultiValueConverter
    {
        private UpBottomSideToRectConverter converter = new UpBottomSideToRectConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pointsObj = converter.Convert(values, targetType, parameter, culture);
            if (pointsObj != null)
            {                
                PointCollection pc = (PointCollection)pointsObj;

                //finding left-top corner
                var left2 = pc.OrderBy(p => p.X).Take(2);
                Point lefttop = left2.OrderBy(p => p.Y).Take(1).Single();


                //finding ridgt-top corner
                var right2 = pc.OrderByDescending(p => p.X).Take(2);
                Point rigthtop = right2.OrderBy(p => p.Y).Take(1).Single();

                //finding left-bottom corner                
                Point leftbottom = left2.OrderByDescending(p => p.Y).Take(1).Single();

                //finding right-bottom corner
                Point rigthbottom = right2.OrderByDescending(p => p.Y).Take(1).Single();

                Point p1, p4;

                //special case
                if ((lefttop.X < leftbottom.X) && (leftbottom.X < rigthtop.X) && (rigthtop.X < rigthbottom.X) && ((lefttop-rigthtop).LengthSquared < (rigthtop-rigthbottom).LengthSquared))
                {
                    p4 = rigthtop;
                    p1 = rigthbottom;
                }
                else
                {
                    if ((lefttop - rigthtop).LengthSquared < (lefttop - leftbottom).LengthSquared)
                    {
                        p4 = leftbottom;
                        p1 = lefttop;
                    }
                    else
                    {
                        p4 = lefttop;
                        p1 = rigthtop;
                    }
                }
                

                TranslateTransform tt = new TranslateTransform(p4.X, p4.Y);

                double angle = 0.0;
                if (Math.Abs(p1.X - p4.X) < 1e-6) { //close to vertical
                    if (p1.Y > p4.Y)
                        angle = 90.0;
                    else
                        angle = -90;
                }
                else {
                    angle = Math.Atan((p1.Y - p4.Y) / (p1.X - p4.X)) / Math.PI * 180.0;
                }

                RotateTransform rt = new RotateTransform(angle);

                TransformGroup tg = new TransformGroup();
                tg.Children.Add(rt);
                tg.Children.Add(tt);

                return tg;
            }
            else
                return null;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class LabelTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null) {
                int order = (int)(values[0]);
                double length = (double)(values[1]);
                double upperDepth = (double)(values[2]);
                double lowerDepth = (double)(values[3]);

                string lengthStr = "не задана";
                if (length > 0.0)
                    lengthStr = string.Format("{0:#.00} м ({1:#.00} м - {2:#.00} м)", length, upperDepth,lowerDepth);
                return string.Format("№{0}: длина {1}", order, lengthStr);
            }
            else return null;
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


        public CalibratedRegionVM AssocaitedCalibratedRegion
        {
            get { return (CalibratedRegionVM)GetValue(AssocaitedCalibratedRegionProperty); }
            set { SetValue(AssocaitedCalibratedRegionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssocaitedCalibratedRegion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssocaitedCalibratedRegionProperty =
            DependencyProperty.Register("AssocaitedCalibratedRegion", typeof(CalibratedRegionVM), typeof(AnnotatedPolygon), new PropertyMetadata(null));



        public AnnotatedPolygon()
        {
            InitializeComponent();
        }        
    }
}
