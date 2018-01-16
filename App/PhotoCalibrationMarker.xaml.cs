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
    public partial class PhotoCalibrationMarker : UserControl, IInfoLayerElement
    {        
        private Point CentreOffset = new Point();        
        
        public PhotoCalibrationMarker()
        {
            InitializeComponent();
            /*this.CentreOffset = new Point(21.0, 21.0);            */
        }        
        
        public Point CentreLocation
        {
            get { return new Point(Canvas.GetLeft(this),Canvas.GetTop(this)); }
        }
        /*
        public static readonly DependencyProperty CentreLocationLocationProperty =
            DependencyProperty.Register("CentreLocation", typeof(Point), typeof(PhotoCalibrationMarker), new PropertyMetadata(new Point(10.0,10.0),((obj,arg) => {
                var p = (Point)arg.NewValue;
                var marker = obj as PhotoCalibrationMarker;
                Point newLocation = new Point(p.X - marker.CentreOffset.X, p.Y - marker.CentreOffset.Y);
                Canvas.SetLeft(marker, newLocation.X);
                Canvas.SetTop(marker, newLocation.Y);
            })));
            */
        public void ChangeCoordTransform(Transform oldTransformInv, Transform newTransform)
        {
            var location = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            var updatedLocation = newTransform.Transform(oldTransformInv.Transform(location));
            Canvas.SetLeft(this, updatedLocation.X);
            Canvas.SetTop(this, updatedLocation.Y);
        }
    }
}