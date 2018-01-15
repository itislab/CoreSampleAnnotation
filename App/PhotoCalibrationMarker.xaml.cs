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
    /// Works with canvas
    /// </summary>
    public partial class PhotoCalibrationMarker : UserControl
    {
        private Transform ActiveCoordsTransform = null;
        private Point CentreOffset = new Point();        
        
        public PhotoCalibrationMarker()
        {
            InitializeComponent();
            Binding bd = new Binding("CentreLocation");
            bd.Source = this;
            bd.Mode = BindingMode.TwoWay;
            this.CentreOffset = new Point(21.0, 21.0);
            this.SetBinding(PhotoCalibrationMarker.InternalCentreLocationProperty, bd);            
        }

        public Brush FillBrash
        {
            get { return (Brush)GetValue(FillBrashProperty); }
            set { SetValue(FillBrashProperty, value); }
        }

        public static readonly DependencyProperty FillBrashProperty =
            DependencyProperty.Register("FillBrash", typeof(Brush), typeof(PhotoCalibrationMarker), new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));



        public Transform CoordsTransform
        {
            get { return (Transform)GetValue(CoordsTransformProperty); }
            set { SetValue(CoordsTransformProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CoordsTransform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoordsTransformProperty =
            DependencyProperty.Register("CoordsTransform", typeof(Transform), typeof(PhotoCalibrationMarker), new PropertyMetadata(null, (obj,args) => {
                if ((args.OldValue != null) && (args.NewValue != null))
                {
                    var marker = obj as PhotoCalibrationMarker;

                    var orig = (args.OldValue as Transform).Inverse;
                    var newVal = args.NewValue as Transform;
                    var centre = marker.InternalCentreLocation;
                    var origCentre = orig.Transform(centre);
                    var newCentre = newVal.Transform(origCentre);
                    marker.InternalCentreLocation = newCentre;
                    //System.Diagnostics.Debug.WriteLine("Marker centre is now at "+newCentre.ToString());
                }
            }));
        

        public Point CentreLocation
        {
            get { return (Point)GetValue(CentreLocationLocationProperty); }
            set { SetValue(CentreLocationLocationProperty, value); }
        }
        
        public static readonly DependencyProperty CentreLocationLocationProperty =
            DependencyProperty.Register("CentreLocation", typeof(Point), typeof(PhotoCalibrationMarker), new PropertyMetadata(new Point(10.0,10.0),((obj,arg) => {
                var p = (Point)arg.NewValue;
                var marker = obj as PhotoCalibrationMarker;
                Point newLocation = new Point(p.X - marker.CentreOffset.X, p.Y - marker.CentreOffset.Y);
                Canvas.SetLeft(marker, newLocation.X);
                Canvas.SetTop(marker, newLocation.Y);
                System.Diagnostics.Debug.WriteLine("Marker target to "+p.ToString());
            })));


        /// <summary>
        /// To be updated with mouse and touch events
        /// </summary>
        private Point InternalCentreLocation
        {
            get { return (Point)GetValue(InternalCentreLocationProperty); }
            set { SetValue(InternalCentreLocationProperty, value); }
        }

        private static readonly DependencyProperty InternalCentreLocationProperty =
            DependencyProperty.Register("InternalCentreLocation", typeof(Point), typeof(PhotoCalibrationMarker), new PropertyMetadata(new Point(10.0, 10.0)));        
    }
}
