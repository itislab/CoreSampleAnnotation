using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

    public enum MarkupState { SettingUp, SettingBottom, SettingSide };

    public enum OrderMoveDirection { Up, Down};

    /// <summary>
    /// Interaction logic for PhotoMarkup.xaml
    /// </summary>
    public partial class PhotoMarkup : UserControl
    {
        private bool isMarkerDragging = false;
        private Point markerClickOffset = new Point();
        private Point dragStartLocation = new Point();
        private PhotoCalibrationMarker currentDraggingMarker = null;

        private ObservableCollection<PhotoCalibrationMarker> canvasMarkers = new ObservableCollection<PhotoCalibrationMarker>();
        private ObservableCollection<AnnotatedPolygon> canvasPolygons = new ObservableCollection<AnnotatedPolygon>();

        private Dictionary<AnnotatedPolygon, CalibratedRegionVM> polygonDict = new Dictionary<AnnotatedPolygon, CalibratedRegionVM>();

        private List<PhotoCalibrationMarker> regionDraft = new List<PhotoCalibrationMarker>();        
        
        private Point touchPoint;
        private bool isMouseDragging = false;

        private Point mouseDownInitialPoint;
        private Timer holdTimer = null;

        public PhotoMarkup()
        {
            InitializeComponent();

            CommonCanvas.MouseMove += CommonCanvas_MouseMove;

            Image.ManipulationStarting += Image_ManipulationStarting;
            Image.ManipulationDelta += Image_ManipulationDelta;

            Image.PreviewTouchDown += Image_TouchDown;
            Image.PreviewTouchUp += Image_TouchUp;
            Image.PreviewTouchMove += Image_TouchMove;

            CommonCanvas.MouseLeftButtonDown += PhotoMarkup_MouseLeftButtonDown;
            CommonCanvas.MouseLeftButtonUp += CommonCanvas_MouseLeftButtonUp;
            CommonCanvas.MouseLeave += CommonCanvas_MouseLeave;


            this.canvasMarkers.CollectionChanged += Markers_CollectionChanged;
            this.canvasPolygons.CollectionChanged += Polygons_CollectionChanged;
            
            //temp: for emulating long touch
            this.CommonCanvas.MouseRightButtonUp += CommonCanvas_MouseRightButtonUp;
            this.Image.MouseWheel += Image_MouseWheel;            
        }

        private void CommonCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseDragging = false;
        }

        private void CommonCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDragging = false;
        }

        private void PhotoMarkup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownInitialPoint = e.GetPosition(CommonCanvas);
            isMouseDragging = true;
        }

        public IEnumerable<CalibratedRegionVM> CalibratedRegions
        {
            get { return (IEnumerable<CalibratedRegionVM>)GetValue(CalibratedRegionsProperty); }
            set { SetValue(CalibratedRegionsProperty, value); }
        }

        public static readonly DependencyProperty CalibratedRegionsProperty =
            DependencyProperty.Register("CalibratedRegions", typeof(IEnumerable<CalibratedRegionVM>), typeof(PhotoMarkup), new PropertyMetadata(new List<CalibratedRegionVM>(), (obj,arg) => {
                PhotoMarkup pm = obj as PhotoMarkup;

                //reseting the control
                pm.UnfocusAllRegions();                
                pm.regionDraft.Clear();
                pm.polygonDict.Clear();
                pm.CurrentState = MarkupState.SettingUp;
                var markers = pm.canvasMarkers.ToArray();
                foreach (var m in markers)
                    pm.canvasMarkers.Remove(m);
                var polygons = pm.canvasPolygons.ToArray();
                foreach (var p in polygons)
                    pm.canvasPolygons.Remove(p);

                //setting up internal state according to the new CalibratedRegions
                IEnumerable<CalibratedRegionVM> vms = arg.NewValue as IEnumerable<CalibratedRegionVM>;

                if (vms != null)
                {
                    foreach (CalibratedRegionVM vm in vms)
                    {
                        AnnotatedPolygon rect = new AnnotatedPolygon();
                        pm.canvasPolygons.Add(rect);

                        rect.DataContext = vm;
                        var upMarker = new PhotoCalibrationMarker() { CentreLocation = vm.Up, MarkerName ="Верх", FillBrush = new SolidColorBrush(new Color() { R = 255, G = 150, B = 150, A = 200 }) };
                        var bottomMarker = new PhotoCalibrationMarker() { CentreLocation = vm.Bottom, MarkerName = "Низ", FillBrush = new SolidColorBrush(new Color() { R = 0, G = 255, B = 255, A = 200 }) };
                        var sideMarker = new PhotoCalibrationMarker() { CentreLocation = vm.Side, MarkerName = "Сторона", FillBrush = new SolidColorBrush(new Color() { R = 255, G = 255, B = 255, A = 200 }) };

                        BindInfoLayer(upMarker, bottomMarker, sideMarker, rect, vm);

                        pm.canvasMarkers.Add(upMarker);
                        pm.canvasMarkers.Add(bottomMarker);
                        pm.canvasMarkers.Add(sideMarker);

                        pm.polygonDict.Add(rect, vm);                        
                    }
                }
            }));
             
        private void Image_TouchMove(object sender, TouchEventArgs e)
        {
            var curTouchPoint = e.GetTouchPoint(CommonCanvas);
            var dist = (touchPoint - curTouchPoint.Position).Length;
            if (dist > 5)
            {
                //diactivating touch and hold
                if (holdTimer != null)
                {
                    System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to move ({0})",dist);
                    holdTimer.Elapsed -= HoldTimer_Elapsed;
                    holdTimer.Stop();
                    holdTimer = null;
                }
            }
        }

        private void HoldTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                PlaceMarker(touchPoint);
            }));
            if (holdTimer != null)
            {
                System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated as it ticked");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }

        private void Image_TouchUp(object sender, TouchEventArgs e)
        {
            if (holdTimer != null)
            {
                System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to up event");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }
        
        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            touchPoint = e.GetTouchPoint(CommonCanvas).Position;
            if (holdTimer != null)
            {
                System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated as the new one starting");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
            holdTimer = new Timer(1000);
            holdTimer.Elapsed += HoldTimer_Elapsed;
            holdTimer.Start();
            System.Diagnostics.Debug.WriteLine("touch-hold timer activated");
            UnfocusAllRegions();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleStep = 1.1;
            var scale = (e.Delta > 0) ? scaleStep : (1 / scaleStep);
            var prevTransform = Image.RenderTransform;

            var mousePosition = e.GetPosition(this);

            var scaleTransform = new ScaleTransform(scale, scale, mousePosition.X, mousePosition.Y);

            TransformGroup group = new TransformGroup();
            group.Children.Add(prevTransform);
            group.Children.Add(scaleTransform);

            var matrix = group.Value;

            Image.RenderTransform = new MatrixTransform(matrix);
        }

        public MarkupState CurrentState
        {
            get { return (MarkupState)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }

        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register("CurrentState", typeof(MarkupState), typeof(PhotoMarkup), new PropertyMetadata(MarkupState.SettingUp));

        /// <summary>
        /// Binds the View (e.g. IInfoLayerElements that are on the CoodTranformedCanvas) to the 
        /// </summary>
        /// <param name="up">Upper point of the core sample main axis</param>
        /// <param name="bottom">Bottom point of the core sample main axis</param>
        /// <param name="side">Any point that is on the side of the core sampe</param>
        /// <param name="poly">An annotated polygon, that highlights the core sample</param>
        /// <param name="vm">View Model that desribes the region</param>
        private static void BindInfoLayer(PhotoCalibrationMarker up, PhotoCalibrationMarker bottom, PhotoCalibrationMarker side, AnnotatedPolygon poly, CalibratedRegionVM vm)
        {
            vm.Up = up.CentreLocation;
            vm.Bottom = bottom.CentreLocation;
            vm.Side = side.CentreLocation;

            var b1 = new Binding(nameof(vm.Up));
            b1.Source = vm;
            b1.Mode = BindingMode.TwoWay;
            up.SetBinding(PhotoCalibrationMarker.CentreLocationLocationProperty, b1);

            var b2 = new Binding(nameof(vm.Bottom));
            b2.Source = vm;
            b2.Mode = BindingMode.TwoWay;
            bottom.SetBinding(PhotoCalibrationMarker.CentreLocationLocationProperty, b2);

            var b3 = new Binding(nameof(vm.Side));
            b3.Source = vm;
            b3.Mode = BindingMode.TwoWay;
            side.SetBinding(PhotoCalibrationMarker.CentreLocationLocationProperty, b3);

            var visConverter = new CollapsedConverter();

            var b4 = new Binding(nameof(vm.AreMarkersVisible));
            b4.Source = vm;
            b4.Converter = visConverter;
            up.SetBinding(UIElement.VisibilityProperty, b4);

            var b5 = new Binding(nameof(vm.AreMarkersVisible));
            b5.Source = vm;
            b5.Converter = visConverter;
            bottom.SetBinding(UIElement.VisibilityProperty, b5);

            var b6 = new Binding(nameof(vm.AreMarkersVisible));
            b6.Source = vm;
            b6.Converter = visConverter;
            side.SetBinding(UIElement.VisibilityProperty, b6);
        }        

        private void CommonCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(CommonCanvas);
            PlaceMarker(position);
        }        

        private void PlaceMarker(Point position)
        {
            PhotoCalibrationMarker toAdd = null;

            switch (CurrentState)
            {
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

            if (regionDraft.Count == 3)
            {
                //draft is ready
                
                CalibratedRegionVM vm = new CalibratedRegionVM();
                vm.Length = double.NaN;
                vm.Up = regionDraft[0].CentreLocation;
                vm.Bottom = regionDraft[1].CentreLocation;
                vm.Side = regionDraft[2].CentreLocation;

                var newCalRegions = new List<CalibratedRegionVM>(CalibratedRegions);

                foreach (PhotoCalibrationMarker marker in regionDraft)
                    canvasMarkers.Remove(marker);

                newCalRegions.Add(vm);                
                CalibratedRegions = newCalRegions;                
                                
                vm.AreMarkersVisible = true;
                FocusedRegion = vm;
            }
            else
            {
                FocusedRegion = null;
                UnfocusAllRegions();
            }
        }        

        private void Poly_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            FocusedRegion = polygonDict[(AnnotatedPolygon)sender];            
            e.Handled = true;
        }




        public CalibratedRegionVM FocusedRegion
        {
            get { return (CalibratedRegionVM)GetValue(FocusedRegionProperty); }
            set { SetValue(FocusedRegionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusedRegion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusedRegionProperty =
            DependencyProperty.Register("FocusedRegion", typeof(CalibratedRegionVM), typeof(PhotoMarkup), new PropertyMetadata(new PropertyChangedCallback((depobj,dpcea) => {
                DependencyPropertyChangedEventArgs dpcea1 = (DependencyPropertyChangedEventArgs)dpcea;
                CalibratedRegionVM vm = dpcea1.NewValue as CalibratedRegionVM;
                CalibratedRegionVM oldVM = dpcea1.OldValue as CalibratedRegionVM;
                if (vm != null)
                {
                    PhotoMarkup pm = depobj as PhotoMarkup;
                    pm.CleanDraftMarkers();
                    pm.UnfocusAllRegions();
                    vm.AreMarkersVisible = true;                    
                    vm.RaiseCanMoveChanged();
                }                
            })));




        private void UnfocusAllRegions()
        {
            foreach (var pair in polygonDict)
            {
                pair.Value.AreMarkersVisible = false;
            }            
        }

        private void CleanDraftMarkers()
        {
            foreach (var marker in regionDraft)
            {
                canvasMarkers.Remove(marker);
            }
            regionDraft.Clear();
            CurrentState = MarkupState.SettingUp;
        }

        private void ActivatePolyFocus(CalibratedRegionVM vm) {
           
        }

        private void Poly_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            FocusedRegion = polygonDict[(AnnotatedPolygon)sender];
            
            e.Handled = true;
        }

        private void Polygons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var obj in e.NewItems)
                {
                    AnnotatedPolygon poly = (AnnotatedPolygon)obj;

                    poly.MouseRightButtonUp += Poly_MouseRightButtonUp;
                    poly.PreviewTouchUp += Poly_PreviewTouchUp;

                    CommonCanvas.Children.Insert(1, poly);
                    Canvas.SetZIndex(poly, 1);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems)
                {
                    AnnotatedPolygon poly = (AnnotatedPolygon)obj;
                    RemovePolygon(poly);
                }
            }            
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
                    marker.PreviewTouchUp += Marker_PreviewTouchDown;

                    CommonCanvas.Children.Add(marker);
                    Canvas.SetZIndex(marker, 3);
                }
            }            
            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems)
                {
                    PhotoCalibrationMarker marker = (PhotoCalibrationMarker)obj;
                    RemoveMarker(marker);
                }
            }            
        }

        private void RemovePolygon(AnnotatedPolygon poly) {
            
            poly.MouseRightButtonUp -= Poly_MouseRightButtonUp;
            poly.PreviewTouchUp -= Poly_PreviewTouchUp;
            CommonCanvas.Children.Remove(poly);
        }

        private void RemoveMarker(PhotoCalibrationMarker marker) {
            marker.MouseDown -= Marker_MouseDown;
            marker.MouseUp -= Marker_MouseUp;
            marker.PreviewTouchUp -= Marker_PreviewTouchDown;
            CommonCanvas.Children.Remove(marker);
        }

        private void Marker_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true;
        }
        
        #region Markers dragging

        private void Marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMarkerDragging = false;
        }

        private void Marker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var marker = (PhotoCalibrationMarker)sender;
            isMarkerDragging = true;
            this.currentDraggingMarker = marker;
            this.markerClickOffset = e.GetPosition(CommonCanvas);
            this.dragStartLocation = marker.CentreLocation;
            e.Handled = true;
        }

        private void CommonCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMarkerDragging)
            {
                var pos = e.GetPosition(CommonCanvas);
                var offset = new Point(pos.X - this.markerClickOffset.X, pos.Y - this.markerClickOffset.Y);
                var newLoc = new Point(this.dragStartLocation.X + offset.X, this.dragStartLocation.Y + offset.Y);
                this.currentDraggingMarker.CentreLocation = newLoc;
            }
            if (isMouseDragging) {
                var pos = e.GetPosition(CommonCanvas);
                var offset = new Point(pos.X - this.mouseDownInitialPoint.X, pos.Y - this.mouseDownInitialPoint.Y);

                Matrix m = Image.RenderTransform.Value;
                m.Append(new TranslateTransform(offset.X,offset.Y).Value);
                mouseDownInitialPoint = pos;
                Image.RenderTransform = new MatrixTransform(m);

            }
        }

        #endregion

        #region image multitouch

        private void Image_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Matrix rectsMatrix = ((MatrixTransform)this.Image.RenderTransform).Matrix;

            //rectsMatrix.RotateAt(e.DeltaManipulation.Rotation, e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

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

    public class SwitchibleTransformBinding : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                bool? switchedOn = (values[0]) as bool?;
                if (switchedOn.HasValue && switchedOn.Value)
                    return values[1] as Transform;
                else
                    return null;
                
            }
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
