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
    /// Works with canvas
    /// </summary>
    public partial class PhotoCalibrationMarker : UserControl, IInfoLayerElement
    {
        private Point CentreOffset = new Point();        
        
        public PhotoCalibrationMarker()
        {
            InitializeComponent();
            this.CentreOffset = new Point(21.0, 21.0);
            this.DataContext = this;

        }

        public Brush FillBrush
        {
            get { return (Brush)GetValue(FillBrushProperty); }
            set { SetValue(FillBrushProperty, value); }
        }

        public static readonly DependencyProperty FillBrushProperty =
            DependencyProperty.Register("FillBrush", typeof(Brush), typeof(PhotoCalibrationMarker), new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));



        public string MarkerName
        {
            get { return (string)GetValue(MarkerNameProperty); }
            set { SetValue(MarkerNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkerName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkerNameProperty =
            DependencyProperty.Register("MarkerName", typeof(string), typeof(PhotoCalibrationMarker), new PropertyMetadata("Marker"));        

        public bool IsNameVisible
        {
            get { return (bool)GetValue(IsNameVisibleProperty); }
            set { SetValue(IsNameVisibleProperty, value); }
        }
        
        public static readonly DependencyProperty IsNameVisibleProperty =
            DependencyProperty.Register("IsNameVisible", typeof(bool), typeof(PhotoCalibrationMarker), new PropertyMetadata(true));



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
                //System.Diagnostics.Debug.WriteLine("Marker target to "+p.ToString());
            })));

        public void ChangeCoordTransform(Transform oldTransformInv, Transform newTransform)
        {
            CentreLocation = newTransform.Transform(oldTransformInv.Transform(CentreLocation));
        }
    }
}
