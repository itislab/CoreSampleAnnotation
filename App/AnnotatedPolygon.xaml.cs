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

namespace All
{
    /// <summary>
    /// Interaction logic for AnnotatedPolygon.xaml
    /// </summary>
    public partial class AnnotatedPolygon : UserControl, IInfoLayerElement
    {
        public AnnotatedPolygon()
        {
            InitializeComponent();
        }
        
        private static Point GetNormalIntersection(Point bottomCentre, Point UpCentre, Point side)
        {
            double y2y1 = UpCentre.Y - bottomCentre.Y;
            double x2x1 = UpCentre.X - bottomCentre.X;

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

        private void UpdateRectCorners() {
            Point T = GetNormalIntersection(BottomCentre, UpCentre, Side);
            Vector toSide = Side - T;
            Point upperLeft = UpCentre - toSide;
            Point upperRight = UpCentre + toSide;
            Point lowerLeft = BottomCentre - toSide;
            Point lowerRight = BottomCentre + toSide;
            Polygon.Points = new PointCollection(new Point[] { upperLeft, upperRight, lowerRight, lowerLeft });
        }

        public Point UpCentre
        {
            get { return (Point)GetValue(UpCentreProperty); }
            set { SetValue(UpCentreProperty, value); }
        }

        public static readonly DependencyProperty UpCentreProperty =
            DependencyProperty.Register("UpCentre", typeof(Point), typeof(AnnotatedPolygon), new PropertyMetadata(new Point(1.0,0.0),(obj,arg) => { ((AnnotatedPolygon)obj).UpdateRectCorners();}));



        public Point BottomCentre
        {
            get { return (Point)GetValue(BottomCentreProperty); }
            set { SetValue(BottomCentreProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BottomCentre.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BottomCentreProperty =
            DependencyProperty.Register("BottomCentre", typeof(Point), typeof(AnnotatedPolygon), new PropertyMetadata(new Point(1.0,1.0), (obj,arg) => { ((AnnotatedPolygon)obj).UpdateRectCorners(); }));



        public Point Side
        {
            get { return (Point)GetValue(SideProperty); }
            set { SetValue(SideProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Side.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SideProperty =
            DependencyProperty.Register("Side", typeof(Point), typeof(AnnotatedPolygon), new PropertyMetadata(new Point(2.0, 0.5), (obj, arg) => { ((AnnotatedPolygon)obj).UpdateRectCorners(); }));
        

        public void ChangeCoordTransform(Transform oldTransformInv, Transform newTransform)
        {
            var points = Polygon.Points;
            Polygon.Points =new PointCollection(points.Select(p => newTransform.Transform(oldTransformInv.Transform(p))));
        }
    }
}
