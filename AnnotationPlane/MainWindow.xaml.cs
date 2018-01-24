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
using InteractiveDataDisplay.WPF;

namespace AnnotationPlane
{
    public class DepthDataTransform : DataTransform
    {
        private Range domain;
        public DepthDataTransform(Range domain) : base(domain)
        {
            this.domain = domain;
        }

        public override double DataToPlot(double dataValue)
        {
            //return 300 * (dataValue - domain.Min) / (domain.Max - domain.Min);
            return 100-dataValue;
        }

        public override double PlotToData(double plotValue)
        {
            //return plotValue * (domain.Max - domain.Min) / 300 + domain.Min;
            return 100+plotValue;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AnnotationGridVM vm = new AnnotationGridVM();
            this.Plane.DataContext = vm;

            DepthAxisColumnVM depthColumnVm = new DepthAxisColumnVM("Шкала шлубин");
            depthColumnVm.LowerBound = -2830;
            depthColumnVm.UpperBound = -2800;
            depthColumnVm.ColumnHeight = 200;

            LayeredColumnVM layerLengthVM = new LayeredColumnVM("Мощность эл-та цикла");
            layerLengthVM.ColumnHeight = 200;
            layerLengthVM.Layers.Add(new LengthLayerVM() { Length = 150});
            layerLengthVM.Layers.Add(new LengthLayerVM() { Length = 50 });

            vm.Columns.Add(depthColumnVm);
            vm.Columns.Add(layerLengthVM);

            //LayeredColumnVM secondColumnVM = new LayeredColumnVM("Числовая характеристика");
            //secondColumnVM.Layers.Add(new )

            //Transform headerRotation = new RotateTransform(-90.0);
            //header1.LayoutTransform = headerRotation;
            //header2.LayoutTransform = headerRotation;
            //header3.LayoutTransform = headerRotation;


            //DepthAxis.DataTransform = new DepthDataTransform(DepthAxis.Range);
        }
    }
}
