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
        private ColumnScale.Controller colScaleController = new ColumnScale.Controller();
        public ColumnScale.Controller ColScaleController {
            get {
                return colScaleController;
            }            
        }

        public MainWindow()
        {
            InitializeComponent();

            AnnotationGridVM vm = new AnnotationGridVM();
            this.Plane.DataContext = vm;
            this.DataContext = this;
            
            ColScaleController.UpperDepth = 2800;
            ColScaleController.LowerDepth = 2830;
            ColScaleController.ScaleFactor = 600.0;

            DepthAxisColumnVM depthColumnVm = new DepthAxisColumnVM("Шкала глубин");
            ColScaleController.AttachToColumn(new ColVMAdapter(depthColumnVm));

            ImageColumnVM imageColumnVm = new ImageColumnVM("Фото керна");            
            imageColumnVm.Source = new BitmapImage(new Uri("core_part.jpg",UriKind.Relative));
            imageColumnVm.ImageUpperDepth = 2801;
            imageColumnVm.ImageLowerDepth = 2802;

            ColScaleController.AttachToColumn(new ColVMAdapter(imageColumnVm));

            LayeredColumnVM layerLengthVM = new LayeredColumnVM("Мощность эл-та цикла");
            ColScaleController.AttachToColumn(new ColVMAdapter(layerLengthVM));

            vm.Columns.Add(depthColumnVm);
            vm.Columns.Add(imageColumnVm);
            vm.Columns.Add(layerLengthVM);            
        }
    }
}
