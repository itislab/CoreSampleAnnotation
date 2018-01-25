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

namespace AnnotationPlane
{    
    /// <summary>
    /// Interaction logic for DepthAxisView.xaml
    /// </summary>
    public partial class DepthAxisView : UserControl        
    {


        public DepthAxisView()
        {
            InitializeComponent();

            Binding b = new Binding("DataContext.Range");
            b.Source = this;
            SetBinding(DepthAxisView.BoundRangeProperty, b);
        }

        

        public InteractiveDataDisplay.WPF.Range BoundRange
        {
            get { return (InteractiveDataDisplay.WPF.Range)GetValue(BoundRangeProperty); }
            set { SetValue(BoundRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoundRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoundRangeProperty =
            DependencyProperty.Register("BoundRange", typeof(InteractiveDataDisplay.WPF.Range), typeof(DepthAxisView), new PropertyMetadata(new InteractiveDataDisplay.WPF.Range(), (obj,args) => {
                DepthAxisView dav = (DepthAxisView)obj;
                InteractiveDataDisplay.WPF.Range? newRange = args.NewValue as InteractiveDataDisplay.WPF.Range?;
                if (newRange.HasValue) {
                    int ticks = (int)((newRange.Value.Max - newRange.Value.Min) * 101);
                    dav.Axis.MaxTicks = ticks;
                }
            }));
    }
}
