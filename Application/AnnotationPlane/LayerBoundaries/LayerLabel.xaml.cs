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
        public Func<IInputElement,Point> GetEventPoint { get; set; }
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
            MouseDown += LayerLabel_MouseDown;
            TouchDown += LayerLabel_TouchDown;

        }

        private void LayerLabel_TouchDown(object sender, TouchEventArgs e)
        {
            LayerBoundary vm = DataContext as LayerBoundary;
            if (vm != null)
            {
                if (vm.DragStarted != null)
                {
                    DragStartEventArgs dsea = new DragStartEventArgs();
                    dsea.FrameworkElement = sender as FrameworkElement;
                    dsea.GetEventPoint = (elem => e.GetTouchPoint(elem).Position);
                    vm.DragStarted.Execute(dsea);
                    e.Handled = true;
                }
            }
        }

        private void LayerLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            LayerBoundary vm = DataContext as LayerBoundary;
            if (vm != null) {
                if (vm.DragStarted != null)
                {
                    DragStartEventArgs dsea = new DragStartEventArgs();
                    dsea.FrameworkElement = sender as FrameworkElement;
                    dsea.GetEventPoint= (elem => e.GetPosition(elem));
                    vm.DragStarted.Execute(dsea);
                    e.Handled = true;
                }
            }
        }
    }
}
