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

        private LayerSyncronization.Controller layerSyncController = new LayerSyncronization.Controller();
        public LayerSyncronization.Controller LayerSyncController {
            get {
                return layerSyncController;
            }            
        }

        private AnnotationGridVM vm = null;


        public MainWindow()
        {
            InitializeComponent();

            LayerSyncController.PropertyChanged += LayerSyncController_PropertyChanged;
            ColScaleController.PropertyChanged += ColScaleController_PropertyChanged;


            vm = new AnnotationGridVM();
            this.Plane.DataContext = vm;
            this.DataContext = this;
            
            ColScaleController.UpperDepth = 2800;
            ColScaleController.LowerDepth = 2830;
            ColScaleController.ScaleFactor = 600.0;

            this.Plane.PointSelected += Plane_PointSelected;


            DepthAxisColumnVM depthColumnVm = new DepthAxisColumnVM("Шкала глубин");
            
            ImageColumnVM imageColumnVm = new ImageColumnVM("Фото керна");            
            imageColumnVm.Source = new BitmapImage(new Uri("core_part.jpg",UriKind.Relative));
            imageColumnVm.ImageUpperDepth = 2801;
            imageColumnVm.ImageLowerDepth = 2802;

            LayerRealLengthColumnVM layerLengthVM = new LayerRealLengthColumnVM("Мощность эл-та цикла");            
            layerLengthVM.Layers.Add(new LengthLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth)  * ColScaleController.ScaleFactor });
                        
            LayeredColumnVM colorColumnVM = new LayeredColumnVM("Цвет");
            colorColumnVM.Layers.Add(new TextLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor, Text = "БежС" });

            LayeredColumnVM textureColumnVM = new LayeredColumnVM("Состав пород");
            textureColumnVM.Layers.Add(new TextLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor, Text = "Пмз" });

            LayeredColumnVM contentColumnVM = new LayeredColumnVM("Текстура");
            contentColumnVM.Layers.Add(new IconLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor});

            vm.Columns.Add(depthColumnVm);
            vm.Columns.Add(imageColumnVm);
            vm.Columns.Add(layerLengthVM);
            vm.Columns.Add(colorColumnVM);
            vm.Columns.Add(textureColumnVM);
            vm.Columns.Add(contentColumnVM);


            ColScaleController.AttachToColumn(new ColVMAdapter(depthColumnVm));
            ColScaleController.AttachToColumn(new ColVMAdapter(imageColumnVm));
            ColScaleController.AttachToColumn(new ColVMAdapter(layerLengthVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(colorColumnVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(textureColumnVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(contentColumnVM));

            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(layerLengthVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(colorColumnVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(textureColumnVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(contentColumnVM));
        }

        private void Plane_PointSelected(object sender, PointSelectedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Selected point with offset {0} in {1} column",e.WpfTopOffset,e.ColumnIdx);

            Type[] newLayerTypes = new Type[] { typeof(ImageColumnVM), typeof(DepthAxisColumnVM) };
            ColumnVM relatedVM = vm.Columns[e.ColumnIdx];
            if (newLayerTypes.Contains(relatedVM.GetType()))
            {
                System.Diagnostics.Debug.WriteLine("Layer split requested");
                layerSyncController.SplitLayer(e.WpfTopOffset);
            }

        }

        private void ColScaleController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(ColScaleController.LowerDepth):
                    LayerSyncController.LowerDepth = ColScaleController.LowerDepth;
                    break;
                case nameof(ColScaleController.UpperDepth):
                    LayerSyncController.UpperDepth = ColScaleController.UpperDepth;
                    break;
                case nameof(ColScaleController.ScaleFactor):
                    LayerSyncController.ScaleFactor = ColScaleController.ScaleFactor;
                    break;
            }
        }

        private void LayerSyncController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LayerSyncController.LowerDepth):
                    ColScaleController.LowerDepth = LayerSyncController.LowerDepth;
                    break;
                case nameof(LayerSyncController.UpperDepth):
                    ColScaleController.UpperDepth = LayerSyncController.UpperDepth;
                    break;
                case nameof(LayerSyncController.ScaleFactor):
                    ColScaleController.ScaleFactor = LayerSyncController.ScaleFactor;
                    break;
            }
        }
    }
}
