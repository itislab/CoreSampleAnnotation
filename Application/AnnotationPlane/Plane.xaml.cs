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

namespace CoreSampleAnnotation.AnnotationPlane
{
    /// <summary>
    /// Interaction logic for Plane.xaml
    /// </summary>
    public partial class Plane : UserControl
    {               
        public Plane()
        {
            InitializeComponent();                                              
            
            this.AnnoGrod.PointSelected += Plane_PointSelected;
            this.AnnoGrod.ElementDropped += AnnoGrod_ElementDropped;

            DataContextChanged += Plane_DataContextChanged;
        }

        private void AnnoGrod_ElementDropped(object sender, ElemDroppedEventArgs e)
        {
            if (ElementDropped != null)
            {
                ElementDropped.Execute(e);
            }
        }

        private void Plane_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PlaneVM mv = e.NewValue as PlaneVM;
            if (mv != null) {
                mv.DragReferenceElem = AnnoGrod.LowerGrid;
            }
        }

        public ICommand PointSelected
        {
            get { return (ICommand)GetValue(PointSelectedProperty); }
            set { SetValue(PointSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointSelectedProperty =
            DependencyProperty.Register("PointSelected", typeof(ICommand), typeof(Plane), new PropertyMetadata(null));



        public ICommand ElementDropped
        {
            get { return (ICommand)GetValue(ElementDroppedProperty); }
            set { SetValue(ElementDroppedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ElementDropped.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ElementDroppedProperty =
            DependencyProperty.Register("ElementDropped", typeof(ICommand), typeof(Plane), new PropertyMetadata(null));



        private void Plane_PointSelected(object sender, PointSelectedEventArgs e)
        {
            if (PointSelected != null) {
                PointSelected.Execute(e);
            }
        }

        
    }
}
