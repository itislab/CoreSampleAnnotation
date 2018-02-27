using System;
using System.Collections.Generic;
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

namespace CoreSampleAnnotation.AnnotationPlane
{    
    /// <summary>
    /// Interaction logic for AnnotationGrid.xaml
    /// </summary>
    public partial class AnnotationGrid : UserControl
    {
        private bool isMouseCaptured = false;

        public AnnotationGrid()
        {
            InitializeComponent();

            Binding b = new Binding("DataContext");
            b.Source = this;
            this.SetBinding(AnnotationGrid.BoundDataContextProperty, b);

            ColumnsGrid.ManipulationStarting += ColumnsGrid_ManipulationStarting;
            ColumnsGrid.ManipulationDelta += ColumnsGrid_ManipulationDelta;


            Binding b2 = new Binding("ScaleFactor");
            b2.Source = this;
            b2.Mode = BindingMode.TwoWay;
            SetBinding(AnnotationGrid.InternalScaleFactorProperty, b2);
            
            LowerGrid.PreviewMouseMove += LowerGrid_PreviewMouseMove;
            LowerGrid.PreviewMouseUp += LowerGrid_PreviewMouseUp;
            LowerGrid.PreviewTouchMove += LowerGrid_PreviewTouchMove;
            LowerGrid.PreviewTouchUp += LowerGrid_PreviewTouchUp;
        }

        private void LowerGrid_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (HandleDragEnd(elem => e.GetTouchPoint(elem).Position))
                e.Handled = true;
        }

        private void LowerGrid_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (HandleDrag(elem => e.GetTouchPoint(elem).Position))                
                e.Handled = true;
        }

        private bool IsInVisualTree(DependencyObject elem, DependencyObject parent) {
            DependencyObject cur = elem;
            do
            {
                if (cur == parent)
                    return true;
                cur = VisualTreeHelper.GetParent(cur);                
            } while (cur != null);
            return false;
        }

        private bool HandleDragEnd(Func<IInputElement,Point> eventPointExtractor) {
            AnnotationGridVM agVM = DataContext as AnnotationGridVM;
            if (agVM != null)
            {
                if (agVM.DraggedItem != null)
                {
                    Point localOffset = agVM.LocalDraggedItemOffset;
                    Point curLocation = eventPointExtractor(ColumnsGrid);
                    Point droppedLocation = new Point(
                        curLocation.X - localOffset.X + agVM.DraggedItem.ActualWidth / 2, //X coord is the location of the centre of the dragged element
                        curLocation.Y - localOffset.Y); //Y coord is a top of the dragged element
                    if (isMouseCaptured)
                    {
                        //ReleaseMouseCapture();
                        System.Diagnostics.Trace.WriteLine("Mouse release");
                        isMouseCaptured = false;
                    }

                    int col = -1;
                    var htResult = VisualTreeHelper.HitTest(ColumnsGrid, droppedLocation);
                    if (htResult != null)
                    {
                        foreach (UIElement elem in ColumnsGrid.Children)
                        {
                            if (IsInVisualTree(htResult.VisualHit, elem))
                                col = Grid.GetColumn(elem);
                        }
                    }


                    ElementDropped?.Invoke(this, new ElemDroppedEventArgs(agVM.DraggedItem, col, droppedLocation.Y));

                    agVM.DraggedItem = null;
                    return true;
                }
            }
            return false;           
        }

        private void LowerGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (HandleDragEnd(elem => e.GetPosition(elem)))
                e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventPointExtractor"></param>
        /// <returns>whether the event is handled</returns>
        private bool HandleDrag(Func<IInputElement, Point> eventPointExtractor) {
            AnnotationGridVM agVM = DataContext as AnnotationGridVM;
            if (agVM != null)
            {
                if (agVM.DraggedItem != null)
                {
                    Point localOffset = agVM.LocalDraggedItemOffset;
                    Point curLocation = eventPointExtractor(LowerGrid);
                    DraggedItemLeft = curLocation.X - localOffset.X;
                    DraggedItemTop = curLocation.Y - localOffset.Y;
                    System.Diagnostics.Trace.WriteLine(string.Format("Elem moved to {0}:{1}", DraggedItemLeft, DraggedItemTop));
                    if (!isMouseCaptured)
                    {
                        //CaptureMouse();
                        System.Diagnostics.Trace.WriteLine("Mouse captured");
                        isMouseCaptured = true;
                    }
                    return true;
                }
            }
            return false;
        }

        private void LowerGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (HandleDrag(elem => e.GetPosition(elem)))
                e.Handled = true;
        }       

        private void ColumnsGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {

            double prefScaleFactor = InternalScaleFactor;
            double scaleFactor = e.DeltaManipulation.Scale.Y;
            if (scaleFactor != 1.0)
            {
                InternalScaleFactor *= scaleFactor;
                //System.Diagnostics.Debug.WriteLine("scale changed by {0:0.0000} to {1:0.##}", scaleFactor, InternalScaleFactor);
            }
            double newScaleFactor = InternalScaleFactor;
            //System.Diagnostics.Debug.WriteLine("scale changed by {2} to {0}, trandlation changed to {1}", InternalScaleFactor, e.DeltaManipulation.Translation.Y,factor);            
            var wpfMo = ColumnsGrid.PointFromScreen(new Point(0.0, e.ManipulationOrigin.Y));//as manipulation origin is in screen coods
            double mo = wpfMo.Y;            
            double startOffset = GridScroll.VerticalOffset;
            double shift = -e.DeltaManipulation.Translation.Y;

            //double startOffsetInScreen = ColumnsGrid.PointToScreen(new Point(0.0, GridScroll.VerticalOffset)).Y;

            //double endOffsetInScreen = startOffset/prefScaleFactor + shi;            

            double endOffsetInWPF = scaleFactor * (startOffset + shift);

            //double newOffset1 = (startOffset-mo+shift)*scaleFactor + mo;

            //System.Diagnostics.Debug.WriteLine("screen mo is {0}; offset {1}l wpf mo is {2}",mo,startOffset, wpfMo.Y);



            GridScroll.ScrollToVerticalOffset(endOffsetInWPF);            
            e.Handled = true;
        }



        public double DraggedItemLeft
        {
            get { return (double)GetValue(DraggedItemLeftProperty); }
            set { SetValue(DraggedItemLeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DraggedItemLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraggedItemLeftProperty =
            DependencyProperty.Register("DraggedItemLeft", typeof(double), typeof(AnnotationGrid), new PropertyMetadata(0.0));




        public double DraggedItemTop
        {
            get { return (double)GetValue(DraggedItemTopProperty); }
            set { SetValue(DraggedItemTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DraggedItemTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraggedItemTopProperty =
            DependencyProperty.Register("DraggedItemTop", typeof(double), typeof(AnnotationGrid), new PropertyMetadata(0.0));



        #region long hold related
        private Point touchPoint;
        private Timer holdTimer = null;
        private ColumnView touchedView = null;

        private void View_TouchMove(object sender, TouchEventArgs e)
        {
            var curTouchPoint = e.GetTouchPoint(ColumnsGrid);
            var dist = (touchPoint - curTouchPoint.Position).Length;
            if (dist > 15)
            {
                //diactivating touch and hold
                if (holdTimer != null)
                {
                    //System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to move ({0})", dist);
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
                int idx = Grid.GetColumn(touchedView);
                PointSelected?.Invoke(this, new PointSelectedEventArgs(idx, touchPoint.Y));
            }));
            if (holdTimer != null)
            {
                //System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated as it ticked");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }


        private void View_TouchLeave(object sender, TouchEventArgs e)
        {
            if (holdTimer != null)
            {
                //System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to touch leave");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }

        private void View_TouchUp(object sender, TouchEventArgs e)
        {
            if (holdTimer != null)
            {
                //System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to up event");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }

        private void View_TouchDown(object sender, TouchEventArgs e)
        {
            touchedView = (ColumnView)sender;

            touchPoint = e.GetTouchPoint(ColumnsGrid).Position;
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
        }


        private void View_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            int idx = Grid.GetColumn(view);

            var position = e.GetPosition(ColumnsGrid);

            PointSelected?.Invoke(this, new PointSelectedEventArgs(idx, position.Y));
        }
        #endregion

        #region multitouch related


        public double ScaleFactor
        {
            get { return (double)GetValue(ScaleFactorProperty); }
            set { SetValue(ScaleFactorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScaleFactor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.Register("ScaleFactor", typeof(double), typeof(AnnotationGrid), new PropertyMetadata(500.0));



        private double InternalScaleFactor
        {
            get { return (double)GetValue(InternalScaleFactorProperty); }
            set { SetValue(InternalScaleFactorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InternalScaleFactor.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty InternalScaleFactorProperty =
            DependencyProperty.Register("InternalScaleFactor", typeof(double), typeof(AnnotationGrid), new PropertyMetadata(500.0));





        private void ColumnsGrid_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;
            //System.Diagnostics.Debug.WriteLine("manipulation started");
        }
        #endregion
        public event EventHandler<PointSelectedEventArgs> PointSelected = null;

        public event EventHandler<ElemDroppedEventArgs> ElementDropped = null;

        public object BoundDataContext
        {
            get { return (object)GetValue(BoundDataContextProperty); }
            set { SetValue(BoundDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoundDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoundDataContextProperty =
            DependencyProperty.Register("BoundDataContext", typeof(object), typeof(AnnotationGrid), new PropertyMetadata(null, AnnotationGrid.DataContexChanged));

        private static void DataContexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnnotationGridVM oldVM = e.NewValue as AnnotationGridVM;
            AnnotationGrid view = (AnnotationGrid)d;
            if (oldVM != null)
            {
                oldVM.Columns.CollectionChanged -= view.Columns_CollectionChanged;
                //TODO: handle columns removal
            }

            AnnotationGridVM newVM = e.NewValue as AnnotationGridVM;
            if (newVM != null)
            {
                newVM.Columns.CollectionChanged += view.Columns_CollectionChanged;
                foreach (ColumnVM colVM in newVM.Columns)
                    view.HandleColumnAdded(colVM);
            }
            
        }

        private void HandleColumnAdded(ColumnVM colVM) {
            //handling column itself
            ColumnView view = new ColumnView();
            view.DataContext = colVM;

            //point selection related
            view.PreviewMouseRightButtonDown += View_MouseRightButtonDown;
            view.TouchDown += View_TouchDown;
            view.TouchMove += View_TouchMove;
            view.TouchUp += View_TouchUp;
            view.IsManipulationEnabled = true;
            view.TouchLeave += View_TouchLeave;

            string sharedWidthGroupName = "annotation_grid_" + Guid.NewGuid().ToString().Replace('-', '_');

            ColumnDefinition cd = new ColumnDefinition();
            cd.SharedSizeGroup = sharedWidthGroupName;
            cd.Width = GridLength.Auto;
            this.ColumnsGrid.ColumnDefinitions.Add(cd);


            Grid.SetColumn(view, this.ColumnsGrid.ColumnDefinitions.Count - 1);
            Grid.SetRow(view, 1);
            this.ColumnsGrid.Children.Add(view);

            //Handling header
            ColumnDefinition header_cd = new ColumnDefinition();
            header_cd.Width = GridLength.Auto;
            header_cd.SharedSizeGroup = sharedWidthGroupName;
            this.HeadersGrid.ColumnDefinitions.Add(header_cd);

            RotateTransform headingRotation = new RotateTransform(-90);
            TextBlock heading = new TextBlock();
            heading.Margin = new Thickness(5);
            heading.HorizontalAlignment = HorizontalAlignment.Center;
            heading.VerticalAlignment = VerticalAlignment.Center;
            heading.Text = colVM.Heading;
            heading.LayoutTransform = headingRotation;
            Border textBorder = new Border() { BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(1) };
            textBorder.Child = heading;

            Grid.SetColumn(textBorder, this.HeadersGrid.ColumnDefinitions.Count - 1);
            Grid.SetRow(textBorder, 0);
            this.HeadersGrid.Children.Add(textBorder);
        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (e.NewItems.Count != 1)
                    throw new InvalidOperationException();

                ColumnVM colVM = e.NewItems[0] as ColumnVM;
                HandleColumnAdded(colVM);
            }
        }
        //todo: handle column removal        
    }

    public class PointSelectedEventArgs: EventArgs
    {
        private double wpfTopOffset;
        private int columnIdx;

        /// <summary>
        /// An index in which the point selection occured
        /// </summary>
        public int ColumnIdx
        {
            get { return columnIdx; }
        }
        /// <summary>
        /// In WPF units (offset from the top of the annotation plane)
        /// </summary>
        public double WpfTopOffset
        {
            get { return wpfTopOffset; }
        }

        internal PointSelectedEventArgs(int columnIdx, double wpfTopOffset)
        {
            this.wpfTopOffset = wpfTopOffset;
            this.columnIdx = columnIdx;
        }
    }

    public class ElemDroppedEventArgs : PointSelectedEventArgs
    {        
        public FrameworkElement DroppedElement { get; private set; }

        public ElemDroppedEventArgs(FrameworkElement fe, int columnIdx, double wpfTopOffset) : base(columnIdx, wpfTopOffset)
        {
            DroppedElement = fe;
        }
    }

    public class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
