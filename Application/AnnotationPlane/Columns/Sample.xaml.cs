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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            PreviewMouseDown += Sample_PreviewMouseDown;
        }

        private void Sample_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("label clicked");
            SampleVM vm = DataContext as SampleVM;
            if (vm != null)
            {
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
