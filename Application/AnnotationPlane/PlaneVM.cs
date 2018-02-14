using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane
{
    struct FullClassID
    {
        public readonly string PropID;
        public readonly string ClassID;

        public FullClassID(string propID, string classID)
        {
            PropID = propID;
            ClassID = classID;
        }
    }


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

        private List<LayeredColumnVM> layerProps = new List<LayeredColumnVM>();
        /// <summary>
        /// The place where actual class annotation is stored
        /// </summary>
        public List<LayeredColumnVM> LayerProps
        {
            get
            {
                return layerProps;
            }
        }

        private List<ColumnVM> scaleSyncedCols = new List<ColumnVM>();
        public List<ColumnVM> ScaleSyncedCols
        {
            get
            {
                return scaleSyncedCols;
            }
        }

        private List<ColumnVM> detachableColumns = new List<ColumnVM>();
        private Dictionary<ColumnVM, ColumnScale.IColumn> scaleAdapters = new Dictionary<ColumnVM, ColumnScale.IColumn>();

        private Dictionary<FullClassID, LayerClassVM> classVMs = new Dictionary<FullClassID, LayerClassVM>();
        private readonly Dictionary<string, IEnumerable<LayerClassVM>> availableClasses = new Dictionary<string, IEnumerable<LayerClassVM>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="toBeRemoved">Whether the column be removed when MassRemove is called</param>
        private void RegisterForScaleSync(ColumnVM column, bool toBeRemoved)
        {
            ColumnScale.IColumn adaptedColumn = new ColVMAdapter(column);
            scaleAdapters.Add(column, adaptedColumn);
            if (toBeRemoved)
            {
                detachableColumns.Add(column);
            }
            ColScaleController.AttachToColumn(adaptedColumn);
        }

        private void RegisterForLayerSync(LayeredColumnVM column)
        {
            LayerSyncronization.ILayersColumn adaptedColumn = new SyncronizerColumnAdapter(column);
            LayerSyncController.RegisterLayer(adaptedColumn);
        }

        private void MassColumnRemove()
        {
            foreach (var column in detachableColumns)
            {
                var adaptedColumn = scaleAdapters[column];
                ColScaleController.DetachColumn(adaptedColumn);
                scaleAdapters.Remove(column);
            }
            detachableColumns.Clear();
        }

        public AnnotationGridVM AnnoGridVM { private set; get; }
        public ClassificationVM classificationVM { private set; get; }

        private ICommand saveImageCommand;

        public ICommand SaveImageCommand
        {
            get { return saveImageCommand; }
            set
            {
                if (saveImageCommand != value)
                {
                    saveImageCommand = value;
                    RaisePropertyChanged(nameof(SaveImageCommand));
                }
            }
        }


        public ICommand PointSelected { private set; get; }

        private ICommand activateSettingsCommand;
        public ICommand ActivateSettingsCommand
        {
            get { return activateSettingsCommand; }
            set
            {
                if (activateSettingsCommand != value)
                {
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

        private readonly Property[] template;

        public PlaneVM(LayersAnnotation annotation, Property[] template)
        {
            this.template = template;

            Init(annotation);
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

        public static LayerClassVM ClassToClassVM(Class cl)
        {
            LayerClassVM result = new LayerClassVM(cl.ID);
            if (cl.Acronym != null)
                result.Acronym = cl.Acronym;
            if (cl.Color != null && cl.Color.HasValue)
                result.Color = cl.Color.Value;
            if (cl.Description != null)
                result.Description = cl.Description;
            if (cl.ShortName != null)
                result.ShortName = cl.ShortName;
            return result;
        }


        /// <summary>
        /// Fills the LayerProps view models with the information passed in annotatioN
        /// </summary>
        /// <param name="annotation"></param>
        private void Init(LayersAnnotation annotation)
        {
            //filing up helper structures
            foreach (Property p in template)
            {
                List<LayerClassVM> availableClassesOfP = new List<LayerClassVM>();
                foreach (Class cl in p.Classes)
                {
                    LayerClassVM lcVM = ClassToClassVM(cl);
                    availableClassesOfP.Add(lcVM);
                    classVMs.Add(new FullClassID(p.ID, lcVM.ID), lcVM); //global dict of classVMs
                }
                availableClasses.Add(p.ID, availableClassesOfP); //global dict of available choices
            }


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
                else if (typeof(LayeredPresentationColumnVM) == relatedVmType)
                {
                    LayeredPresentationColumnVM lpvm = (LayeredPresentationColumnVM)relatedVM;
                    int layerIdx = layerSyncController.GetLayerIndex(psea.WpfTopOffset);
                    ClassificationLayerPresentingVM clpmv = lpvm.Layers[layerIdx] as ClassificationLayerPresentingVM;
                    if (clpmv != null)
                    {
                        classificationVM.LayerVM = clpmv.ClassificationVM;
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
                LayerClassVM lc = (LayerClassVM)fe.DataContext;
                classificationVM.LayerVM.CurrentClass = lc;
            });

            classificationVM.IsVisible = false;

            double[] boundaries = annotation.LayerBoundaries;
            int layersCount = boundaries.Length - 1;

            double upperDepth = annotation.LayerBoundaries[0];
            double lowerDepth = annotation.LayerBoundaries[annotation.LayerBoundaries.Length - 1];

            ColScaleController.UpperDepth = upperDepth;
            LayerSyncController.UpperDepth = upperDepth;
            ColScaleController.LowerDepth = lowerDepth;
            LayerSyncController.LowerDepth = lowerDepth;
            LayerSyncController.SetColumnDepth(upperDepth, lowerDepth);

            double colHeight = LayerSyncController.DepthToWPF(lowerDepth) - LayerSyncController.DepthToWPF(upperDepth);

            foreach (var property in template)
            {
                LayeredColumnVM columnVM = new LayeredColumnVM(property.ID);
                for (int i = 0; i < layersCount; i++)
                {
                    ClassificationLayerVM layerVM = new ClassificationLayerVM();
                    layerVM.PossibleClasses = availableClasses[property.ID];
                    ColumnValues column = annotation.Columns.FirstOrDefault(c => c.PropID == property.ID);
                    if (column != null) //setting choice                        
                    {
                        FullClassID fullID = new FullClassID(column.PropID, column.LayerValues[i].Value);
                        if (classVMs.ContainsKey(fullID))
                        {
                            layerVM.CurrentClass = classVMs[fullID];
                        }
                    }                    
                    layerVM.Length = LayerSyncController.LengthToWPF(boundaries[i + 1] - boundaries[i]);
                    columnVM.Layers.Add(layerVM);
                }
                columnVM.ColumnHeight = colHeight;
                columnVM.UpperBound = upperDepth;
                columnVM.LowerBound = lowerDepth;
                RegisterForLayerSync(columnVM);
                RegisterForScaleSync(columnVM, false);
                LayerProps.Add(columnVM);
            }
        }

        public void SetPresentationColumns(ColumnSettingsVM columnDefinitions, Intervals.PhotoRegion[] photos)
        {
            double colHeight = LayerSyncController.LengthToWPF(ColScaleController.LowerDepth - ColScaleController.UpperDepth);

            //reseting previous selected columns
            AnnoGridVM = new AnnotationGridVM();

            MassColumnRemove();

            //now adding the new ones
            foreach (var columnDefinition in columnDefinitions.OrderedColumnDefinitions)
            {
                if (columnDefinition is DepthColumnDefinitionVM)
                {
                    DepthAxisColumnVM colVM = new DepthAxisColumnVM("Шкала глубин");
                    colVM.ColumnHeight = colHeight;
                    AnnoGridVM.Columns.Add(colVM);
                    RegisterForScaleSync(colVM, true);
                }
                else if (columnDefinition is LayerLengthColumnDefinitionVM)
                {
                    LayerRealLengthColumnVM colVM = new LayerRealLengthColumnVM("Мощность эл-та циклита (м)");
                    colVM.ColumnHeight = colHeight;
                    for (int i = 0; i < LayerSyncController.DepthBoundaries.Length - 1; i++)
                    {
                        LengthLayerVM llVM = new LengthLayerVM();
                        llVM.Length = LayerSyncController.LengthToWPF(LayerSyncController.DepthBoundaries[i + 1] - LayerSyncController.DepthBoundaries[i]);
                        colVM.Layers.Add(llVM);
                    }

                    AnnoGridVM.Columns.Add(colVM);
                    RegisterForScaleSync(colVM, true);
                    RegisterForLayerSync(colVM);
                }
                else if (columnDefinition is PhotoColumnDefinitionVM)
                {
                    ImageColumnVM imColVM = new ImageColumnVM("Фото керна");
                    imColVM.ColumnHeight = colHeight;
                    imColVM.ImageRegions = photos;
                    AnnoGridVM.Columns.Add(imColVM);
                    RegisterForScaleSync(imColVM, true);
                }
                else if (columnDefinition is LayeredTextColumnDefinitionVM)
                {
                    LayeredTextColumnDefinitionVM colDef = (LayeredTextColumnDefinitionVM)columnDefinition;
                    if (colDef.SelectedCentreTextProp != null)
                    {
                        string propID = colDef.SelectedCentreTextProp.PropID;

                        LayeredColumnVM propColumnVM = LayerProps.Where(p => p.Heading == propID).Single();

                        //Creating presentation column.

                        //here is the actual displayed text is set
                        Func<LayerClassVM, string> centreTextExtractor;
                        switch (colDef.SelectedCentreTextProp.Presentation)
                        {
                            case Presentation.Acronym: centreTextExtractor = vm1 => vm1.Acronym; break;
                            case Presentation.Description: centreTextExtractor = vm1 => vm1.Description; break;
                            case Presentation.ShortName: centreTextExtractor = vm1 => vm1.ShortName; break;
                            case Presentation.Colour: throw new InvalidOperationException();
                            default: throw new NotImplementedException();
                        }

                        LayeredPresentationColumnVM colVM = new LayeredPresentationColumnVM(
                            colDef.SelectedCentreTextProp.TexturalString,
                            propColumnVM,
                            lVM => new ClassificationLayerTextPresentingVM((ClassificationLayerVM)lVM) { TextExtractor = centreTextExtractor });

                        colVM.ColumnHeight = colHeight;

                        AnnoGridVM.Columns.Add(colVM);
                    }
                }
                else throw new NotSupportedException("Незнакомое определение колонки");
            }
        }
    }
}
