using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class PlaneVM : ViewModel
    {
        private Template.Property[] activeTemplate;

        public Template.Property[] ActiveTemplate
        {
            get { return activeTemplate; }
            set
            {
                if (activeTemplate != value)
                {
                    activeTemplate = value;
                    RaisePropertyChanged(nameof(ActiveTemplate));
                }
            }
        }

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

        private ICommand activateSettingsCommand;
        public ICommand ActivateSettingsCommand {
            get { return activateSettingsCommand; }
            set {
                if (activateSettingsCommand != value) {
                    activateSettingsCommand = value;
                    RaisePropertyChanged(nameof(ActivateSettingsCommand));
                }
            }
        }

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

        public ImageColumnVM ImageColumnVM { get; private set; }

        public void Init()
        {
            LayerRealLengthColumnVM layerLengthVM = new LayerRealLengthColumnVM("Мощность эл-та цикла");
            layerLengthVM.Layers.Add(new LengthLayerVM() { Length = (ColScaleController.LowerDepth - ColScaleController.UpperDepth) * ColScaleController.ScaleFactor });

            Property colourProperty = new Property();
            colourProperty.Name = "Цвет";
            colourProperty.IsMulticlass = false;
            colourProperty.Classes = new Class[] {
                new Class() { Acronym="c",  ShortName="серые" , Color= Colors.Gray},
                new Class() { Acronym="бж", ShortName= "бежевые", Color= Colors.Beige},
                new Class() { Acronym="бе", ShortName= "белесые", Color= Colors.AntiqueWhite},
                new Class() { Acronym="бу", ShortName= "бурые", Color= Colors.SaddleBrown},
                new Class() { Acronym="ч",  ShortName="чёрные", Color= Colors.Black},
                new Class() { Acronym="к",  ShortName="коричневые", Color= Colors.Brown},
                new Class() { Acronym="ж",  ShortName="жёлтые", Color= Colors.Yellow},
                new Class() { Acronym="ст", ShortName= "темно-серые", Color= Colors.DarkGray},
                new Class() { Acronym="сс", ShortName= "светло-серые", Color= Colors.LightGray},
                new Class() { Acronym="б",  ShortName="белые", Color= Colors.White},
                new Class() { Acronym="г",  ShortName="голубые", Color= Colors.LightBlue},
                new Class() { Acronym="з",  ShortName="зеленые", Color= Colors.Green},
                new Class() { Acronym="кр", ShortName= "красные", Color= Colors.Red},
                new Class() { Acronym="сн", ShortName= "сиреневые", Color= Colors.Violet},
                new Class() { Acronym="рз", ShortName= "розовые", Color= Colors.Pink},
                new Class() { Acronym="рж", ShortName= "рыжие", Color= Colors.OrangeRed},
                new Class() { Acronym="ф",  ShortName="фиолетовые", Color= Colors.Purple},
                new Class() { Acronym="х",  ShortName="хаки", Color= Colors.Khaki},
                new Class() { Acronym="тж", ShortName= "темно-желтые", Color= Colors.DarkGoldenrod},
                new Class() { Acronym="жс", ShortName= "светло-желтые", Color= Colors.LightYellow},
                new Class() { Acronym="зс", ShortName= "светло-зеленые", Color= Colors.LightGreen},
                new Class() { Acronym="нз", ShortName= "сине-зеленые", Color= Colors.Turquoise},
                new Class() { Acronym="гс", ShortName= "светло-голубые", Color= Colors.LightSkyBlue},
                new Class() { Acronym="гт", ShortName= "темно-голубые", Color= Colors.DarkBlue},
                new Class() { Acronym="кв", ShortName= "светло-коричневые", Color= Colors.SandyBrown},
                new Class() { Acronym="кт", ShortName= "темно-коричневые", Color= Colors.RosyBrown},
                new Class() { Acronym="хс", ShortName= "светлый хаки", Color= Colors.Khaki},
                new Class() { Acronym="тх", ShortName= "темный хаки", Color= Colors.DarkKhaki}
            };

            Property textureProperty = new Property();
            textureProperty.Name = "Текстура";
            textureProperty.IsMulticlass = false;
            textureProperty.Classes = new Class[] {
                new Class() { Acronym="б",  ShortName="Бугристая"},
                new Class() { Acronym="в",  ShortName="Восходящая рябь"},
                new Class() { Acronym="гв", ShortName= "Горизонтальная волнистая"},
                new Class() { Acronym="гл", ShortName= "Горизонтальная линзовидная"},
                new Class() { Acronym="гп", ShortName= "Горизонтальная параллельная"},
                new Class() { Acronym="гш", ShortName= "Градационная штормового накопления"},
                new Class() { Acronym="зв", ShortName= "Знаки волновой ряби"},
                new Class() { Acronym="зт", ShortName= "Знакопеременных течений"},
                new Class() { Acronym="кв", ShortName= "Косослоистая волнистая"},
                new Class() { Acronym="кл", ShortName= "Косослоистая линзовидная"},
                new Class() { Acronym="кп", ShortName= "Косослоистая параллельная однонаправленная"},
                new Class() { Acronym="кр", ShortName= "Косослоистая параллельная разнонаправленная"},
                new Class() { Acronym="м",  ShortName="Массивная"},
                new Class() { Acronym="му", ShortName= "Мульдообразная"},
                new Class() { Acronym="нб", ShortName= "Нарушенная брекчированием"},
                new Class() { Acronym="ил", ShortName= "Нарушенная илоедами", InputFields = new InputField[] {
                    new PropertyInputField() {
                        Property = new Property() {
                            Name = "Биотурбация",
                            IsMulticlass = false,
                            Classes = new Class[] {
                            new Class() { Acronym="н",  ShortName="незначительная" , Description = "единичные ходы"},
                            new Class() { Acronym="у", ShortName= "умеренная"},
                            new Class() { Acronym="и", ShortName= "интенсивная", Description = "порода почти полностью переработана илоедами"}
                            }
                        }
                    }
                } }
            };


            ActiveTemplate = new Property[] { colourProperty, textureProperty };

            ColScaleController.AttachToColumn(new ColVMAdapter(layerLengthVM));            
            LayerSyncController.RegisterLayer(new SyncronizerColumnAdapter(layerLengthVM));            
        }

        public PlaneVM()
        {
            AnnoGridVM = new AnnotationGridVM();
            classificationVM = new ClassificationVM();

            PointSelected = new DelegateCommand(obj =>
            {
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

            AnnoGridVM.Columns.Add(depthColumnVm);

            ColScaleController.AttachToColumn(new ColVMAdapter(depthColumnVm));

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
