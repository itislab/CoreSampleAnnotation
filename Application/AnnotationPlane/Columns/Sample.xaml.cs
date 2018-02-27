using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
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


namespace CoreSampleAnnotation.AnnotationPlane.Columns
{
    /// <summary>
    /// Interaction logic for Sample.xaml
    /// </summary>
    public partial class Sample : UserControl
    {
        public Sample()
        {
            InitializeComponent();            
            MouseDown += Sample_MouseDown;
            TouchDown += Sample_TouchDown;
            
        }

        private void Sample_MouseDown(object sender, MouseEventArgs e)
        {
            SampleVM vm = DataContext as SampleVM;
            if (vm != null)
            {
                if (vm.DragStarted != null)
                {
                    DragStartEventArgs dsea = new DragStartEventArgs();
                    dsea.FrameworkElement = sender as FrameworkElement;
                    dsea.GetEventPoint = (elem => e.GetPosition(elem));
                    vm.DragStarted.Execute(dsea);
                    e.Handled = true;
                }
            }
        }

        private void Sample_TouchDown(object sender, TouchEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("label touched");
            SampleVM vm = DataContext as SampleVM;
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
    }
}
