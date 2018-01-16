using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private PhotoCalibrationMarker currentDraggingMarker = null;

        private ObservableCollection<PhotoCalibrationMarker> markers = new ObservableCollection<PhotoCalibrationMarker>();        

        public PhotoMarkup()
        {
            InitializeComponent();

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

            this.markers.CollectionChanged += Markers_CollectionChanged;

            var coordsTransformBinding = new Binding("RenderTransform");
            coordsTransformBinding.Source = Image;
            CommonCanvas.SetBinding(CoordTransformedCanvas.CoordsTransformProperty, coordsTransformBinding);


            var a = new PhotoCalibrationMarker() { CentreLocation = new Point(15.0, 15.0), MarkerName = "Верх", FillBrush = new SolidColorBrush(new Color() { R = 255, G = 150, B = 150, A = 200 }) };
            var b = new PhotoCalibrationMarker() { CentreLocation = new Point(15.0, 255.0), MarkerName = "Низ", FillBrush = new SolidColorBrush(new Color() { R = 0, G = 255, B = 255, A = 200 }) };
            var c = new PhotoCalibrationMarker() { CentreLocation = new Point(270.0, 75.0), MarkerName = "Сторона", FillBrush = new SolidColorBrush(new Color() { R = 255, G = 255, B = 255, A = 200 }) };
            
            markers.Add(a);
            markers.Add(b);
            markers.Add(c);

            AnnotatedPolygon rect = new AnnotatedPolygon();
            CommonCanvas.Children.Add(rect);

            CalibratedRegion region = new CalibratedRegion(a, b, c, rect);

        }

        private void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //all of the markers respect coords transform
            if (e.NewItems != null)
            {
                foreach (var obj in e.NewItems)
                {
                    PhotoCalibrationMarker marker = (PhotoCalibrationMarker)obj;
                    
                    marker.MouseDown += Marker_MouseDown;
                    marker.MouseUp += Marker_MouseUp;

                    CommonCanvas.Children.Add(marker);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems)
                {
                    PhotoCalibrationMarker marker = (PhotoCalibrationMarker)obj;
                    
                    CommonCanvas.Children.Remove(marker);
                }
            }
        }



        #region Markers dragging

        private void Marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void Marker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var marker = (PhotoCalibrationMarker)sender;
            isDragging = true;
            this.currentDraggingMarker = marker;
            this.markerClickOffset = e.GetPosition(CommonCanvas);
            this.dragStartLocation = marker.CentreLocation;
            e.Handled = true;
        }

        private void InfoLayerCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var pos = e.GetPosition(CommonCanvas);
                var offset = new Point(pos.X - this.markerClickOffset.X, pos.Y - this.markerClickOffset.Y);
                var newLoc = new Point(this.dragStartLocation.X + offset.X, this.dragStartLocation.Y + offset.Y);
                this.currentDraggingMarker.CentreLocation = newLoc;                
            }
        }

        #endregion

        #region image multitouch

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

        #endregion        
    }
}
