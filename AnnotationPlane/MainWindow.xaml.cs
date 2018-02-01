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
        private ClassificationVM classificationVM = null;


        public MainWindow()
        {
            InitializeComponent();

            LayerSyncController.PropertyChanged += LayerSyncController_PropertyChanged;
            ColScaleController.PropertyChanged += ColScaleController_PropertyChanged;


            vm = new AnnotationGridVM();
            this.Plane.DataContext = vm;
            this.DataContext = this;

            Binding sfBind1 = new Binding("ScaleFactor");
            sfBind1.Source = ColScaleController;
            sfBind1.Mode = BindingMode.OneWayToSource;
            Plane.SetBinding(AnnotationGrid.ScaleFactorProperty, sfBind1);

            Binding sfBind2 = new Binding("ScaleFactor");
            sfBind2.Source = LayerSyncController;
            sfBind2.Mode = BindingMode.OneWayToSource;
            Plane.SetBinding(AnnotationGrid.ScaleFactorProperty, sfBind2);


            ColScaleController.UpperDepth = 2800;
            ColScaleController.LowerDepth = 2830;
            ColScaleController.ScaleFactor = 3000.0;

            this.Plane.PointSelected += Plane_PointSelected;


            DepthAxisColumnVM depthColumnVm = new DepthAxisColumnVM("Шкала глубин");
            
            ImageColumnVM imageColumnVm = new ImageColumnVM("Фото керна");            
            imageColumnVm.Source = new BitmapImage(new Uri("core_part.jpg",UriKind.Relative));
            imageColumnVm.ImageUpperDepth = 2801;
            imageColumnVm.ImageLowerDepth = 2802;

            LayerRealLengthColumnVM layerLengthVM = new LayerRealLengthColumnVM("Мощность эл-та цикла");            
            layerLengthVM.Layers.Add(new LengthLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth)  * ColScaleController.ScaleFactor });
                                    
            LayeredColumnVM textureColumnVM = new LayeredColumnVM("Состав пород");
            textureColumnVM.Layers.Add(new IconLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor });

            LayeredColumnVM bioColumnVM = new LayeredColumnVM("Биотурбация");
            var bioLayer = new ClassificationLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor };
            bioLayer.PossibleClasses.Add(new LayerClass("н", "незначительная - единичные ходы"));
            bioLayer.PossibleClasses.Add(new LayerClass("у", "умеренная"));
            bioLayer.PossibleClasses.Add(new LayerClass("и", "интенсивная - порода почти полностью переработана илоедами"));
            bioColumnVM.Layers.Add(bioLayer);


            LayeredColumnVM colorColumnVM = new LayeredColumnVM("Цвет");
            var colorLayer = new ClassificationLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor };
            colorLayer.PossibleClasses.Add(new LayerClass("c", "серые"));
            colorLayer.PossibleClasses.Add(new LayerClass("бж", "бежевые"));
            colorLayer.PossibleClasses.Add(new LayerClass("бе", "белесые"));
            colorLayer.PossibleClasses.Add(new LayerClass("бу", "бурые"));
            colorLayer.PossibleClasses.Add(new LayerClass("ч", "чёрные"));
            colorLayer.PossibleClasses.Add(new LayerClass("к", "коричневые"));
            colorLayer.PossibleClasses.Add(new LayerClass("ж", "жёлтые"));
            colorLayer.PossibleClasses.Add(new LayerClass("ст", "темно-серые"));
            colorLayer.PossibleClasses.Add(new LayerClass("сс", "светло-серые"));
            colorLayer.PossibleClasses.Add(new LayerClass("б", "белые"));
            colorLayer.PossibleClasses.Add(new LayerClass("г", "голубые"));
            colorLayer.PossibleClasses.Add(new LayerClass("з", "зеленые"));
            colorLayer.PossibleClasses.Add(new LayerClass("кр", "красные"));
            colorLayer.PossibleClasses.Add(new LayerClass("сн", "сиреневые"));
            colorLayer.PossibleClasses.Add(new LayerClass("рз", "розовые"));
            colorLayer.PossibleClasses.Add(new LayerClass("рж", "рыжие"));
            colorLayer.PossibleClasses.Add(new LayerClass("ф", "фиолетовые"));
            colorLayer.PossibleClasses.Add(new LayerClass("х", "хаки"));
            colorLayer.PossibleClasses.Add(new LayerClass("тж", "темно-желтые"));
            colorLayer.PossibleClasses.Add(new LayerClass("жс", "светло-желтые"));
            colorLayer.PossibleClasses.Add(new LayerClass("зс", "светло-зеленые"));
            colorLayer.PossibleClasses.Add(new LayerClass("нз", "сине-зеленые"));
            colorLayer.PossibleClasses.Add(new LayerClass("гс", "светло-голубые"));
            colorLayer.PossibleClasses.Add(new LayerClass("гт", "темно-голубые"));
            colorLayer.PossibleClasses.Add(new LayerClass("кв", "светло-коричневые"));
            colorLayer.PossibleClasses.Add(new LayerClass("кт", "темно-коричневые"));
            colorLayer.PossibleClasses.Add(new LayerClass("хс", "светлый хаки"));
            colorLayer.PossibleClasses.Add(new LayerClass("тх", "темный хаки"));
            colorColumnVM.Layers.Add(colorLayer);


            LayeredColumnVM contentColumnVM = new LayeredColumnVM("Текстура");
            var textureLayer = new ClassificationLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor };
            textureLayer.PossibleClasses.Add(new LayerClass("б", "Бугристая"));
            textureLayer.PossibleClasses.Add(new LayerClass("в", "Восходящая рябь"));
            textureLayer.PossibleClasses.Add(new LayerClass("гв", "Горизонтальная волнистая"));
            textureLayer.PossibleClasses.Add(new LayerClass("гл", "Горизонтальная линзовидная"));
            textureLayer.PossibleClasses.Add(new LayerClass("гп", "Горизонтальная параллельная"));
            textureLayer.PossibleClasses.Add(new LayerClass("гш", "Градационная штормового накопления"));
            textureLayer.PossibleClasses.Add(new LayerClass("зв", "Знаки волновой ряби"));
            textureLayer.PossibleClasses.Add(new LayerClass("зт", "Знакопеременных течений"));
            textureLayer.PossibleClasses.Add(new LayerClass("кв", "Косослоистая волнистая"));
            textureLayer.PossibleClasses.Add(new LayerClass("кл", "Косослоистая линзовидная"));
            textureLayer.PossibleClasses.Add(new LayerClass("кп", "Косослоистая параллельная однонаправленная"));
            textureLayer.PossibleClasses.Add(new LayerClass("кр", "Косослоистая параллельная разнонаправленная"));
            textureLayer.PossibleClasses.Add(new LayerClass("м", "Массивная"));
            textureLayer.PossibleClasses.Add(new LayerClass("му", "Мульдообразная"));
            textureLayer.PossibleClasses.Add(new LayerClass("нб", "Нарушенная брекчированием"));
            contentColumnVM.Layers.Add(textureLayer);

            vm.Columns.Add(depthColumnVm);
            vm.Columns.Add(imageColumnVm);
            vm.Columns.Add(layerLengthVM);
            vm.Columns.Add(colorColumnVM);
            vm.Columns.Add(textureColumnVM);
            vm.Columns.Add(contentColumnVM);
            vm.Columns.Add(bioColumnVM);


            ColScaleController.AttachToColumn(new ColVMAdapter(depthColumnVm));
            ColScaleController.AttachToColumn(new ColVMAdapter(imageColumnVm));
            ColScaleController.AttachToColumn(new ColVMAdapter(layerLengthVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(colorColumnVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(textureColumnVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(contentColumnVM));
            ColScaleController.AttachToColumn(new ColVMAdapter(bioColumnVM));

            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(layerLengthVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(colorColumnVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(textureColumnVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(contentColumnVM));
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(bioColumnVM));


            classificationVM = new ClassificationVM();            
            classificationVM.CloseCommand = new DelegateCommand(() => classificationVM.IsVisible = false);
            classificationVM.ClassSelectedCommand = new DelegateCommand(sender =>
            {
                FrameworkElement fe = sender as FrameworkElement;
                LayerClass lc = (LayerClass)fe.DataContext;
                classificationVM.LayerVM.CurrentClass = lc;
            });
            ClassificationView.DataContext = classificationVM;
        }

        private void Plane_PointSelected(object sender, PointSelectedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Selected point with offset {0} in {1} column",e.WpfTopOffset,e.ColumnIdx);
            
            Type[] newLayerTypes = new Type[] { typeof(ImageColumnVM), typeof(DepthAxisColumnVM) };
            ColumnVM relatedVM = vm.Columns[e.ColumnIdx];
            Type relatedVmType = relatedVM.GetType();
            if (newLayerTypes.Contains(relatedVmType))
            {
                System.Diagnostics.Debug.WriteLine("Layer split requested");
                layerSyncController.SplitLayer(e.WpfTopOffset);
            } else if (typeof(LayeredColumnVM) == relatedVmType) {
                LayeredColumnVM lcvm = (LayeredColumnVM)relatedVM;
                int layerIdx = layerSyncController.GetLayerIndex(e.WpfTopOffset);
                ClassificationLayerVM clmv = lcvm.Layers[layerIdx] as ClassificationLayerVM;
                if (clmv != null) {
                    classificationVM.LayerVM = clmv;
                    classificationVM.IsVisible = true;
                }                
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
