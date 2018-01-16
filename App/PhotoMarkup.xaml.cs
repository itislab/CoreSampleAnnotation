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

    public enum MarkupState { SettingUp, SettingBottom,SettingSide};

    /// <summary>
    /// Interaction logic for PhotoMarkup.xaml
    /// </summary>
    public partial class PhotoMarkup : UserControl
    {
        private bool isDragging = false;
        private Point markerClickOffset = new Point();
        private Point dragStartLocation = new Point();
        private PhotoCalibrationMarker currentDraggingMarker = null;

        private ObservableCollection<PhotoCalibrationMarker> canvasMarkers = new ObservableCollection<PhotoCalibrationMarker>();
        private ObservableCollection<AnnotatedPolygon> canvasPolygons = new ObservableCollection<AnnotatedPolygon>();

        private List<PhotoCalibrationMarker> regionDraft = new List<PhotoCalibrationMarker>();

        private List<CalibratedRegion> regions = new List<CalibratedRegion>();

        private double angle = 0.0;

        public PhotoMarkup()
        {
            InitializeComponent();

            CommonCanvas.MouseMove += InfoLayerCanvas_MouseMove;

            Image.ManipulationStarting += Image_ManipulationStarting;
            Image.ManipulationDelta += Image_ManipulationDelta;

            this.canvasMarkers.CollectionChanged += Markers_CollectionChanged;
            this.canvasPolygons.CollectionChanged += Polygons_CollectionChanged;

            var coordsTransformBinding = new Binding("RenderTransform");
            coordsTransformBinding.Source = Image;
            CommonCanvas.SetBinding(CoordTransformedCanvas.CoordsTransformProperty, coordsTransformBinding);

            //temp: for emulating long touch
            this.CommonCanvas.MouseRightButtonUp += CommonCanvas_MouseRightButtonUp;

            this.Image.MouseWheel += Image_MouseWheel;
            
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta * 0.01;
            angle += delta;
            Image.RenderTransform = new RotateTransform(angle);
        }

        public MarkupState CurrentState
        {
            get { return (MarkupState)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }
        
        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register("CurrentState", typeof(MarkupState), typeof(PhotoMarkup), new PropertyMetadata(MarkupState.SettingUp));



        private void CommonCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            PhotoCalibrationMarker toAdd = null;

            Point position = e.GetPosition(CommonCanvas);

            switch (CurrentState) {
                case MarkupState.SettingUp:
                    toAdd = new PhotoCalibrationMarker() { CentreLocation = position, MarkerName = "Верх", FillBrush = new SolidColorBrush(new Color() { R = 255, G = 150, B = 150, A = 200 }) };
                    CurrentState = MarkupState.SettingBottom;
                    break;
                case MarkupState.SettingBottom:
                    toAdd = new PhotoCalibrationMarker() { CentreLocation = position, MarkerName = "Низ", FillBrush = new SolidColorBrush(new Color() { R = 0, G = 255, B = 255, A = 200 }) };
                    CurrentState = MarkupState.SettingSide;
                    break;
                case MarkupState.SettingSide:
                    toAdd = new PhotoCalibrationMarker() { CentreLocation = position, MarkerName = "Сторона", FillBrush = new SolidColorBrush(new Color() { R = 255, G = 255, B = 255, A = 200 }) };
                    CurrentState = MarkupState.SettingUp;
                    break;
            }
            canvasMarkers.Add(toAdd);
            regionDraft.Add(toAdd);

            if (regionDraft.Count == 3) {
                //draft is ready
                AnnotatedPolygon rect = new AnnotatedPolygon();
                CommonCanvas.Children.Add(rect);

                CalibratedRegion region = new CalibratedRegion(regionDraft[0], regionDraft[1], regionDraft[2], rect);
                regions.Add(region);
                regionDraft.Clear();
            }

        }

        private void Polygons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var obj in e.NewItems)
                {
                    AnnotatedPolygon poly = (AnnotatedPolygon)obj;
                    
                    poly.MouseUp += Poly_MouseUp;

                    CommonCanvas.Children.Insert(1,poly);
                    Canvas.SetZIndex(poly,1);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems)
                {
                    AnnotatedPolygon poly = (AnnotatedPolygon)obj;
                    poly.MouseUp -= Poly_MouseUp;
                    CommonCanvas.Children.Remove(poly);
                }
            }
        }

        private void Poly_MouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
        
        private void Markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {            
            if (e.NewItems != null)
            {
                foreach (var obj in e.NewItems)
                {
                    PhotoCalibrationMarker marker = (PhotoCalibrationMarker)obj;
                    
                    marker.MouseDown += Marker_MouseDown;
                    marker.MouseUp += Marker_MouseUp;

                    CommonCanvas.Children.Add(marker);
                    Canvas.SetZIndex(marker, 2);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems)
                {
                    PhotoCalibrationMarker marker = (PhotoCalibrationMarker)obj;
                    marker.MouseDown -= Marker_MouseDown;
                    marker.MouseUp -= Marker_MouseUp;
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
