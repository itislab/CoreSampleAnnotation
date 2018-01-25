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

namespace AnnotationPlane
{    
    /// <summary>
    /// Interaction logic for AnnotationGrid.xaml
    /// </summary>
    public partial class AnnotationGrid : UserControl
    {
        public AnnotationGrid()
        {
            InitializeComponent();

            Binding b = new Binding("DataContext");
            b.Source = this;
            this.SetBinding(AnnotationGrid.BoundDataContextProperty, b);            
        }

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
                    System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to move ({0})", dist);
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
                System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated as it ticked");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }


        private void View_TouchLeave(object sender, TouchEventArgs e)
        {
            if (holdTimer != null)
            {
                System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to touch leave");
                holdTimer.Elapsed -= HoldTimer_Elapsed;
                holdTimer.Stop();
                holdTimer = null;
            }
        }

        private void View_TouchUp(object sender, TouchEventArgs e)
        {
            if (holdTimer != null)
            {
                System.Diagnostics.Debug.WriteLine("touch-hold timer diactivated due to up event");
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

        public event EventHandler<PointSelectedEventArgs> PointSelected = null;

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
            }
        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (e.NewItems.Count != 1)
                    throw new InvalidOperationException();

                ColumnVM colVM = e.NewItems[0] as ColumnVM;

                //handling column itself
                ColumnView view = new ColumnView();
                view.DataContext = colVM;

                //point selection related
                view.PreviewMouseRightButtonDown += View_MouseRightButtonDown;
                view.PreviewTouchDown += View_TouchDown;
                view.PreviewTouchMove += View_TouchMove;
                view.PreviewTouchUp += View_TouchUp;
                view.TouchLeave += View_TouchLeave;

                string sharedWidthGroupName = "annotation_grid_"+Guid.NewGuid().ToString().Replace('-','_');

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

            //todo: handle column removal
        }
    }

    public class PointSelectedEventArgs
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
}
