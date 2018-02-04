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
        }



        public ICommand PointSelected
        {
            get { return (ICommand)GetValue(PointSelectedProperty); }
            set { SetValue(PointSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointSelectedProperty =
            DependencyProperty.Register("PointSelected", typeof(ICommand), typeof(Plane), new PropertyMetadata(null));



        private void Plane_PointSelected(object sender, PointSelectedEventArgs e)
        {
            if (PointSelected != null) {
                PointSelected.Execute(e);
            }
        }

        
    }
}
