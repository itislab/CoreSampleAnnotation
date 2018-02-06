using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CoreSampleAnnotation.AnnotationPlane
{
    /// <summary>
    /// This is an image that exposes actual width as read/write prop dep (to enable setting of oneWayToSourceBinding)
    /// see: https://stackoverflow.com/questions/658170/onewaytosource-binding-from-readonly-property-in-xaml#658683
    /// </summary>
    public class RegionImage : Image
    {
        public RegionImage()
        {
            Binding b = new Binding("ActualWidth");
            b.Source = this;
            SetBinding(InternalActualWidthProperty, b);            
            
        }
        

        private double InternalActualWidth
        {
            get { return (double)GetValue(InternalActualWidthProperty); }
            set { SetValue(InternalActualWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualWidth2.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty InternalActualWidthProperty =
            DependencyProperty.Register("InternalActualWidth", typeof(double), typeof(RegionImage),new PropertyMetadata((o,e) => {
                RegionImage ri = o as RegionImage;
                System.Diagnostics.Debug.WriteLine("actual width updated from {0} to {1}",e.OldValue,e.NewValue);

                ri.ActualWidth2 = (double)e.NewValue;
                //dont know why, but the changes do not propagate automatically, so forcing source update
                //BindingExpression be = ri.GetBindingExpression(ActualWidth2Property);
                //be.UpdateSource();
                //System.Diagnostics.Debug.WriteLine("source updated");                
            }));



        public double ActualWidth2
        {
            get { return (double)GetValue(ActualWidth2Property); }
            set { SetValue(ActualWidth2Property, value); }
        }

        // Using a DependencyProperty as the backing store for ActualWidth2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualWidth2Property =
            DependencyProperty.Register("ActualWidth2", typeof(double), typeof(RegionImage), new PropertyMetadata(0.0));




    }
}
