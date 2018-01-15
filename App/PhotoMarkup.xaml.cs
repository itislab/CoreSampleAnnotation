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
    /// Interaction logic for PhotoMarkup.xaml
    /// </summary>
    public partial class PhotoMarkup : UserControl
    {
        private bool isDragging = false;
        private Point markerClickOffset = new Point();
        private Point dragStartLocation = new Point();

        //private TransformGroup imageTransform;
        //private TranslateTransform imageTranslation;
        //private ScaleTransform imageScale;
        //private RotateTransform imageRotation;
        
        public PhotoMarkup()
        {
            InitializeComponent();                        
            Marker.CentreLocation = new Point(320, 180);
            //CommonCanvas.MouseRightButtonDown += Image_MouseRightButtonUp;
            //CommonCanvas.MouseLeftButtonDown += Image_MouseLeftButtonUp;
            Marker.MouseDown += Marker_MouseDown;
            Marker.MouseUp += Marker_MouseUp;
            CommonCanvas.MouseMove += InfoLayerCanvas_MouseMove;

            //this.imageTranslation = new TranslateTransform(0, 0);
            //this.imageScale = new ScaleTransform(0.5, 0.5);
            //this.imageRotation = new RotateTransform(0.0);

            //this.imageTransform = new TransformGroup();
            //this.imageTransform.Children.Add(this.imageRotation);
            //this.imageTransform.Children.Add(this.imageScale);
            //this.imageTransform.Children.Add(this.imageTranslation);
            //this.Image.RenderTransform = this.imageTransform;

            Image.ManipulationStarting += Image_ManipulationStarting;
            Image.ManipulationDelta += Image_ManipulationDelta;

            var coordsTransformBinding = new Binding("RenderTransform");
            coordsTransformBinding.Source = Image;

            this.Marker.SetBinding(PhotoCalibrationMarker.CoordsTransformProperty, coordsTransformBinding);
        }

        private void Marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void InfoLayerCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging) {
                var pos = e.GetPosition(CommonCanvas);
                var offset = new Point(pos.X - this.markerClickOffset.X, pos.Y - this.markerClickOffset.Y);
                var newLoc = new Point(this.dragStartLocation.X + offset.X, this.dragStartLocation.Y + offset.Y);
                this.Marker.CentreLocation = newLoc;                
            }
        }

        private void Image_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Matrix rectsMatrix = ((MatrixTransform)this.Image.RenderTransform).Matrix;

            rectsMatrix.RotateAt(e.DeltaManipulation.Rotation, e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

            rectsMatrix.ScaleAt(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.X,
               e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

            rectsMatrix.Translate(e.DeltaManipulation.Translation.X,
               e.DeltaManipulation.Translation.Y);

            this.Image.RenderTransform = new MatrixTransform(rectsMatrix);

            e.Handled = true;
        }

        private void Image_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;
            System.Diagnostics.Debug.WriteLine("manipulation started");
        }

        private void Marker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            this.markerClickOffset = e.GetPosition(CommonCanvas);
            this.dragStartLocation = Marker.CentreLocation;
            e.Handled = true;
        }        
    }
}
