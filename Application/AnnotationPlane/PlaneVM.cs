using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class PlaneVM : ViewModel
    {
        private ColumnScale.Controller colScaleController = new ColumnScale.Controller();
        public ColumnScale.Controller ColScaleController
        {
            get
            {
                return colScaleController;
            }
        }

        private LayerSyncronization.Controller layerSyncController = new LayerSyncronization.Controller();
        public LayerSyncronization.Controller LayerSyncController
        {
            get
            {
                return layerSyncController;
            }

        }

        public AnnotationGridVM AnnoGridVM { private set; get; }
        public ClassificationVM classificationVM { private set; get; }

        public ICommand PointSelected { private set; get; }

        /// <summary>
        /// How much is 1 real meter in WPF coordinates
        /// </summary>
        public double ScaleFactor
        {
            get
            {
                return ColScaleController.ScaleFactor;
            }
            set
            {
                if (value != ColScaleController.ScaleFactor)
                {
                    ColScaleController.ScaleFactor = value;
                    layerSyncController.ScaleFactor = value;
                }
            }
        }

        public void Init() {
            LayerRealLengthColumnVM layerLengthVM = new LayerRealLengthColumnVM("Мощность эл-та цикла");
            layerLengthVM.Layers.Add(new LengthLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor });

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

            AnnoGridVM.Columns.Add(layerLengthVM);
            AnnoGridVM.Columns.Add(colorColumnVM);
            AnnoGridVM.Columns.Add(textureColumnVM);
            AnnoGridVM.Columns.Add(contentColumnVM);
            AnnoGridVM.Columns.Add(bioColumnVM);


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
        }

        public PlaneVM()
        {
            AnnoGridVM = new AnnotationGridVM();
            classificationVM = new ClassificationVM();
            
            PointSelected = new DelegateCommand(obj => {
                PointSelectedEventArgs psea = obj as PointSelectedEventArgs;
                System.Diagnostics.Debug.WriteLine("Selected point with offset {0} in {1} column", psea.WpfTopOffset, psea.ColumnIdx);

                Type[] newLayerTypes = new Type[] { typeof(ImageColumnVM), typeof(DepthAxisColumnVM) };
                ColumnVM relatedVM = AnnoGridVM.Columns[psea.ColumnIdx];
                Type relatedVmType = relatedVM.GetType();
                if (newLayerTypes.Contains(relatedVmType))
                {
                    System.Diagnostics.Debug.WriteLine("Layer split requested");
                    layerSyncController.SplitLayer(psea.WpfTopOffset);
                }
                else if (typeof(LayeredColumnVM) == relatedVmType)
                {
                    LayeredColumnVM lcvm = (LayeredColumnVM)relatedVM;
                    int layerIdx = layerSyncController.GetLayerIndex(psea.WpfTopOffset);
                    ClassificationLayerVM clmv = lcvm.Layers[layerIdx] as ClassificationLayerVM;
                    if (clmv != null)
                    {
                        classificationVM.LayerVM = clmv;
                        classificationVM.IsVisible = true;
                    }
                }
            });

            LayerSyncController.PropertyChanged += LayerSyncController_PropertyChanged;
            ColScaleController.PropertyChanged += ColScaleController_PropertyChanged;

            ColScaleController.UpperDepth = 2800;
            ColScaleController.LowerDepth = 2830;
            ColScaleController.ScaleFactor = 3000.0;

            classificationVM = new ClassificationVM();
            classificationVM.CloseCommand = new DelegateCommand(() => classificationVM.IsVisible = false);
            classificationVM.ClassSelectedCommand = new DelegateCommand(sender =>
            {
                FrameworkElement fe = sender as FrameworkElement;
                LayerClass lc = (LayerClass)fe.DataContext;
                classificationVM.LayerVM.CurrentClass = lc;
            });

            classificationVM.IsVisible = false;

            DepthAxisColumnVM depthColumnVm = new DepthAxisColumnVM("Шкала глубин");

            ImageColumnVM imageColumnVm = new ImageColumnVM("Фото керна");
            imageColumnVm.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("core_part.jpg", UriKind.Relative));
            imageColumnVm.ImageUpperDepth = 2801;
            imageColumnVm.ImageLowerDepth = 2802;

            AnnoGridVM.Columns.Add(depthColumnVm);
            AnnoGridVM.Columns.Add(imageColumnVm);

            ColScaleController.AttachToColumn(new ColVMAdapter(depthColumnVm));
            ColScaleController.AttachToColumn(new ColVMAdapter(imageColumnVm));

        }

        private void ColScaleController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
