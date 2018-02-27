using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
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


    [Serializable]
    public class PlaneVM : ViewModel, ISerializable
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

        private LayerBoundaryEditorVM layerBoundaryEditorVM;
        private FrameworkElement dragReferenceElem;
        /// <summary>
        /// Coordinates dragging elements are calculated related to this element
        /// </summary>
        public FrameworkElement DragReferenceElem
        {
            get { return dragReferenceElem; }
            set
            {
                if (dragReferenceElem != value)
                {
                    dragReferenceElem = value;
                    RaisePropertyChanged(nameof(DragReferenceElem));
                }
            }
        }

        /// <param name="layerIdx">index of inner (between layers) boundaries</param>
        private void RemoveLayerBoundary(int layerIdx, LayerSyncronization.FreeSpaceAccepter freeSpaceAccepter)
        {
            layerBoundaryEditorVM.RemoveBoundary(layerIdx);
            switch (freeSpaceAccepter)
            {
                case LayerSyncronization.FreeSpaceAccepter.LowerLayer:
                    LayerSyncController.RemoveLayer(layerIdx, freeSpaceAccepter); break;
                case LayerSyncronization.FreeSpaceAccepter.UpperLayer:
                    LayerSyncController.RemoveLayer(layerIdx + 1, freeSpaceAccepter); break;
                default: throw new NotImplementedException();
            }

        }

        /// <param name="layerIdx">index of inner (between layers) boundaries</param>
        /// <param name="level">in WPF units</param>
        private void MoveLayerBoundary(int layerIdx, double level)
        {
            layerBoundaryEditorVM.MoveBoundary(layerIdx, level);
            double up = 0.0;
            if (layerIdx > 0)
                up = LayerSyncController.GetLayerWPFTop(layerIdx);
            LayerSyncController.MoveBoundary(layerIdx, level - up);
        }

        private void AddBoundary(double level)
        {
            layerBoundaryEditorVM.AddBoundary(0, level);
            LayerSyncController.SplitLayer(level);
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

        private SamplesColumnVM samplesColumnVM;

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
            LayerSyncController.RegisterColumn(adaptedColumn);
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

        private ICommand elementDropped;
        public ICommand ElementDropped
        {
            get
            {
                return elementDropped;
            }
            set
            {
                if (elementDropped != value)
                {
                    elementDropped = value;
                    RaisePropertyChanged(nameof(ElementDropped));
                }
            }
        }

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

        private readonly ILayersTemplateSource layersTemplateSource;

        public PlaneVM(LayersAnnotation annotation, ILayersTemplateSource layersTemplateSource)
        {
            this.layersTemplateSource = layersTemplateSource;
            this.layerBoundaryEditorVM = new LayerBoundaryEditorVM();
            samplesColumnVM = new SamplesColumnVM();
            Initialize(annotation);
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
        private void Initialize(LayersAnnotation annotation)
        {
            //filing up helper structures
            foreach (Property p in layersTemplateSource.Template)
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


            ElementDropped = new DelegateCommand(obj =>
            {
                ElemDroppedEventArgs edea = obj as ElemDroppedEventArgs;
                LayerBoundary boundary = edea.DroppedElement.Tag as LayerBoundary;
                SampleVM sample = edea.DroppedElement.Tag as SampleVM;
                if (boundary != null)
                {
                    System.Diagnostics.Debug.WriteLine("dropped boundary {0} with offset {1} in {2} column", boundary.ID, edea.WpfTopOffset, edea.ColumnIdx);
                    int boundIdx = Array.FindIndex(layerBoundaryEditorVM.Boundaries, b => b.ID == boundary.ID);
                    double prevTop = layerSyncController.GetLayerWPFTop(boundIdx);
                    double prevBottom = layerSyncController.GetLayerWPFBottom(boundIdx);
                    double nextBottom = layerSyncController.GetLayerWPFBottom(boundIdx + 1);
                    double newBottom = edea.WpfTopOffset;
                    if (newBottom <= prevTop)
                    {
                        var res = MessageBox.Show(
                            string.Format("Удалить границу слоев на глубине {0:#.##}м заполнив путое место информацией из слоя снизу?", layerSyncController.WpfToDepth(boundary.Level)),
                            "Удаление слоя", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        switch (res)
                        {
                            case MessageBoxResult.OK:
                                RemoveLayerBoundary(boundIdx, LayerSyncronization.FreeSpaceAccepter.LowerLayer);
                                break;
                            case MessageBoxResult.Cancel:
                                layerBoundaryEditorVM.Boundaries = layerBoundaryEditorVM.Boundaries.ToArray(); // this recreates all views, after the boundary label dragging
                                break;
                            default: throw new NotImplementedException();
                        }

                    }
                    else if (newBottom >= nextBottom)
                    {
                        var res = MessageBox.Show(
                        string.Format("Удалить границу слоев на глубине {0:#.##}м заполнив путое место информацией из слоя сверху?", layerSyncController.WpfToDepth(boundary.Level)),
                        "Удаление слоя", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        switch (res)
                        {
                            case MessageBoxResult.OK:
                                RemoveLayerBoundary(boundIdx, LayerSyncronization.FreeSpaceAccepter.UpperLayer);
                                break;
                            case MessageBoxResult.Cancel:
                                layerBoundaryEditorVM.Boundaries = layerBoundaryEditorVM.Boundaries.ToArray(); // this recreates all views, after the boundary label dragging
                                break;
                            default: throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        //dealing with rank change
                        int colIdx = edea.ColumnIdx;
                        if (colIdx != -1)
                        {
                            //drop is in one of the column
                            BoundaryEditorColumnVM becVM = AnnoGridVM.Columns[colIdx] as BoundaryEditorColumnVM;
                            if (becVM != null)
                            {
                                //this is boundary editor column. Setting rank according to the column
                                RankMatchingBoundaryCollection rfbc = becVM.BoundariesVM as RankMatchingBoundaryCollection;
                                if (rfbc != null)
                                {
                                    //this column is attached to rank filtering VM
                                    layerBoundaryEditorVM.ChangeRank(boundary.ID, rfbc.Rank);
                                }
                            }
                        }
                        System.Diagnostics.Debug.WriteLine("Changing layer {0} bottom from {1} to {2}", boundIdx, prevBottom, edea.WpfTopOffset);
                        MoveLayerBoundary(boundIdx, edea.WpfTopOffset);
                    }
                }
                else if (sample != null)
                {
                    System.Diagnostics.Debug.WriteLine("dropped smaple with offset {0}", edea.WpfTopOffset);
                    if ((edea.ColumnIdx != -1) && (AnnoGridVM.Columns[edea.ColumnIdx] is SamplesColumnVM))
                    {
                        double depth = layerSyncController.WpfToDepth(edea.WpfTopOffset + 50);//TODO: grap center coord of the UI from UI, not from hardcoded value of 50
                        sample.Depth = depth;
                        samplesColumnVM.Samples = samplesColumnVM.Samples.ToArray(); //asignment forces refresh
                    }
                    else {                        
                        var res = MessageBox.Show(
                            string.Format("Удалить образец {0} с глубины {1:#.##}м?",sample.Number, sample.Depth),
                            "Подтверждение удаления образца",
                            MessageBoxButton.OKCancel,MessageBoxImage.Question);
                        switch (res) {
                            case MessageBoxResult.OK:
                                samplesColumnVM.Samples = samplesColumnVM.Samples.Where(s => s != sample).ToArray();
                                break;
                            case MessageBoxResult.Cancel:
                                samplesColumnVM.Samples = samplesColumnVM.Samples.ToArray(); //asignment forces refresh
                                break;
                            default: throw new NotImplementedException();
                        }
                    }
                }
                else throw new NotImplementedException("Сброшен незнакомый объект");
            });

            PointSelected = new DelegateCommand(obj =>
            {
                PointSelectedEventArgs psea = obj as PointSelectedEventArgs;
                System.Diagnostics.Debug.WriteLine("Selected point with offset {0} in {1} column", psea.WpfTopOffset, psea.ColumnIdx);

                Type[] newLayerTypes = new Type[] { typeof(ImageColumnVM), typeof(DepthAxisColumnVM), typeof(BoundaryEditorColumnVM) };
                ColumnVM relatedVM = AnnoGridVM.Columns[psea.ColumnIdx];
                Type relatedVmType = relatedVM.GetType();
                if (newLayerTypes.Contains(relatedVmType))
                {
                    System.Diagnostics.Debug.WriteLine("Layer split requested");
                    AddBoundary(psea.WpfTopOffset);
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
                else if (typeof(SamplesColumnVM) == relatedVmType)
                {
                    SamplesColumnVM scVM = (SamplesColumnVM)relatedVM;
                    double depth = layerSyncController.WpfToDepth(psea.WpfTopOffset);
                    SampleVM sampleVM = new SampleVM(depth);
                    List<SampleVM> l = new List<SampleVM>(scVM.Samples);
                    l.Add(sampleVM);
                    scVM.Samples = l.ToArray();
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
                if (classificationVM.LayerVM is SingleClassificationLayerVM)
                {
                    SingleClassificationLayerVM sclVM = (SingleClassificationLayerVM)classificationVM.LayerVM;
                    sclVM.CurrentClass = lc;
                }
                else if (classificationVM.LayerVM is MultiClassificationLayerVM)
                {
                    MultiClassificationLayerVM mclVM = (MultiClassificationLayerVM)classificationVM.LayerVM;
                    HashSet<LayerClassVM> curClasses;
                    if (mclVM.CurrentClasses == null)
                        curClasses = new HashSet<LayerClassVM>();
                    else
                        curClasses = new HashSet<LayerClassVM>(mclVM.CurrentClasses);
                    if (curClasses.Contains(lc))
                        curClasses.Remove(lc);
                    else
                        curClasses.Add(lc);
                    mclVM.CurrentClasses = curClasses;
                }
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

            layerBoundaryEditorVM.DragStart = new DelegateCommand(args =>
            {
                if (DragReferenceElem != null)
                {
                    DragStartEventArgs dsea = args as DragStartEventArgs;
                    Point startDragCoord = dsea.GetEventPoint(DragReferenceElem);
                    Point localDragOffset = dsea.GetEventPoint(dsea.FrameworkElement);
                    Point dragItemLocation = startDragCoord - new Vector(localDragOffset.X, localDragOffset.Y);
                    AnnoGridVM.LocalDraggedItemOffset = localDragOffset;
                    AnnoGridVM.DragItemLocation = dragItemLocation;
                    dsea.FrameworkElement.Tag = dsea.FrameworkElement.DataContext; // storing original data context in tag befor the element is moved out of the UI tree
                    AnnoGridVM.DraggedItem = dsea.FrameworkElement;
                }
            });

            samplesColumnVM.DragStart = layerBoundaryEditorVM.DragStart;

            double colHeight = LayerSyncController.DepthToWPF(lowerDepth) - LayerSyncController.DepthToWPF(upperDepth);

            foreach (var property in layersTemplateSource.Template)
            {
                LayeredColumnVM columnVM = new LayeredColumnVM(property.ID);
                for (int i = 0; i < layersCount; i++)
                {
                    ClassificationLayerVM layerVM;
                    if (property.IsMulticlass)
                    {
                        layerVM = new MultiClassificationLayerVM();
                    }
                    else
                    {
                        layerVM = new SingleClassificationLayerVM();
                    }
                    layerVM.PossibleClasses = availableClasses[property.ID];
                    ColumnValues column = annotation.Columns.FirstOrDefault(c => c.PropID == property.ID);
                    if (column != null) //setting choice                        
                    {
                        if (column.LayerValues[i].Value != null)
                        {
                            foreach (string classID in column.LayerValues[i].Value)
                            {
                                FullClassID fullID = new FullClassID(column.PropID, classID);
                                if (classVMs.ContainsKey(fullID))
                                {
                                    if (property.IsMulticlass)
                                    {
                                        MultiClassificationLayerVM mclVM = (MultiClassificationLayerVM)layerVM;
                                        List<LayerClassVM> classes = new List<LayerClassVM>();
                                        if (mclVM.CurrentClasses != null)
                                            classes = new List<LayerClassVM>(mclVM.CurrentClasses);
                                        classes.Add(classVMs[fullID]);
                                        mclVM.CurrentClasses = classes;
                                    }
                                    else
                                    {
                                        SingleClassificationLayerVM sclVM = (SingleClassificationLayerVM)layerVM;
                                        sclVM.CurrentClass = classVMs[fullID];
                                    }

                                }
                                if (!property.IsMulticlass)
                                    break;
                            }
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

            samplesColumnVM.ColumnHeight = colHeight;
            RegisterForScaleSync(samplesColumnVM, false);

            var syncAdapter = new SyncronizerBoundaryAdapter(layerBoundaryEditorVM);

            //            LayerSyncController.RegisterColumn(syncAdapter);
            ColScaleController.AttachToColumn(syncAdapter);
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
                    //BoundaryEditorColumnVM beVM = new BoundaryEditorColumnVM(imColVM, layerBoundaryEditorVM);

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
                            lVM =>
                            {
                                if (lVM is SingleClassificationLayerVM)
                                    return new SingleClassificationLayerTextPresentingVM((SingleClassificationLayerVM)lVM) { TextExtractor = centreTextExtractor };
                                else if (lVM is MultiClassificationLayerVM)
                                    return new MultiClassificationLayerTextPresentingVM((MultiClassificationLayerVM)lVM) { TextExtractor = centreTextExtractor };
                                else throw new InvalidOperationException("Unexpected VM type");
                            });

                        colVM.ColumnHeight = colHeight;

                        AnnoGridVM.Columns.Add(colVM);
                    }
                }
                else if (columnDefinition is LayerEditColumnDefinitionVM)
                {
                    LayerEditColumnDefinitionVM colDef = (LayerEditColumnDefinitionVM)columnDefinition;
                    int rank = colDef.SelectedIndex;
                    string heading = string.Format("Границы между {0}", colDef.Selected.ToLowerInvariant());
                    BlankColumnVM blankColumnVM = new BlankColumnVM(heading);
                    RankMoreOrEqualBoundaryCollection filter = new RankMoreOrEqualBoundaryCollection(layerBoundaryEditorVM, rank);
                    RankMatchingBoundaryCollection filter2 = new RankMatchingBoundaryCollection(layerBoundaryEditorVM, rank);

                    BoundaryLineColumnVM blVM = new BoundaryLineColumnVM(blankColumnVM, filter);
                    BoundaryEditorColumnVM beVM = new BoundaryEditorColumnVM(blVM, filter2);


                    blankColumnVM.ColumnHeight = colHeight;
                    blankColumnVM.ColumnWidth = 100;
                    AnnoGridVM.Columns.Add(beVM);
                    RegisterForScaleSync(blankColumnVM, true);
                }
                else if (columnDefinition is LayerSamplesDefinitionVM)
                {
                    AnnoGridVM.Columns.Add(samplesColumnVM);
                }
                else throw new NotSupportedException("Незнакомое определение колонки");
            }
        }

        #region Persistence

        protected PlaneVM(SerializationInfo info, StreamingContext context)
        {
            LayersAnnotation layersAnnotation = (LayersAnnotation)info.GetValue("LayersAnnotation", typeof(LayersAnnotation));
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplate", typeof(ILayersTemplateSource));
            layerBoundaryEditorVM = (LayerBoundaryEditorVM)info.GetValue("LayerBoundaries", typeof(LayerBoundaryEditorVM));
            samplesColumnVM = (SamplesColumnVM)info.GetValue("Samples",typeof(SamplesColumnVM));
            Initialize(layersAnnotation);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //Dumping layer prop VMs to LayersAnnotation
            LayersAnnotation layersAnnotation = new LayersAnnotation();
            layersAnnotation.LayerBoundaries = LayerSyncController.DepthBoundaries;
            layersAnnotation.Columns = LayerProps.Select(col =>
            {
                List<LayerPropertyValue> values = new List<LayerPropertyValue>();
                foreach (var layer in col.Layers)
                {
                    ClassificationLayerVM clVM = layer as ClassificationLayerVM;
                    LayerPropertyValue v = new LayerPropertyValue();
                    if (clVM is SingleClassificationLayerVM)
                    {
                        SingleClassificationLayerVM sclVM = (SingleClassificationLayerVM)clVM;
                        if (sclVM.CurrentClass != null)
                        {
                            v.Value = new string[] { sclVM.CurrentClass.ID };
                        }
                    }
                    else if (clVM is MultiClassificationLayerVM)
                    {
                        MultiClassificationLayerVM mclVM = (MultiClassificationLayerVM)clVM;
                        {
                            if (mclVM.CurrentClasses != null)
                            {
                                v.Value = mclVM.CurrentClasses.Select(c => c.ID).ToArray();
                            }
                        }
                    }
                    else throw new InvalidOperationException("Unexpected VM type");

                    values.Add(v);
                }
                return new ColumnValues() { PropID = col.Heading, LayerValues = values.ToArray() };
            }).ToArray();
            info.AddValue("Samples",samplesColumnVM);
            info.AddValue("LayersAnnotation", layersAnnotation);
            info.AddValue("LayersTemplate", layersTemplateSource);
            info.AddValue("LayerBoundaries", layerBoundaryEditorVM);
        }

        #endregion
    }
}
