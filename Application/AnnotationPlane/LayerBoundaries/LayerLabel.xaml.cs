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

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    public class DragStartEventArgs : EventArgs {
        public MouseEventArgs MouseEvent { get; set; }
        public FrameworkElement FrameworkElement { get; set; }
    }

    /// <summary>
    /// Interaction logic for LayerLabel.xaml
    /// </summary>
    public partial class LayerLabel : UserControl
    {
        public LayerLabel()
        {
            InitializeComponent();
            this.PreviewMouseDown += Rect_PreviewMouseDown;
            this.PreviewMouseMove += LayerLabel_PreviewMouseMove;
        }

        private void LayerLabel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = false;            
        }

        private void Rect_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("label clicked");
            LayerBoundary vm = DataContext as LayerBoundary;
            if (vm != null) {
                if (vm.DragStarted != null)
                {
                    DragStartEventArgs dsea = new DragStartEventArgs();
                    dsea.FrameworkElement = sender as FrameworkElement;
                    dsea.MouseEvent = e;
                    vm.DragStarted.Execute(dsea);
                }
            }
        }
    }
}
