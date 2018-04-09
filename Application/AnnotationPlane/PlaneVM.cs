using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using CoreSampleAnnotation.AnnotationPlane.Template;
using CoreSampleAnnotation.Reports.SVG;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        private AnnotationDirection annotationDirection;
        /// <summary>
        /// The direction in which the layers are numbered.        
        /// </summary>
        public AnnotationDirection AnnotationDirection
        {
            get { return annotationDirection; }
            set
            {
                if (annotationDirection != value)
                {
                    annotationDirection = value;
                    RaisePropertyChanged(nameof(AnnotationDirection));
                    allLayersBoundaryEditorVM.AnnotationDirection = value;
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

        private LayerBoundaryEditorVM allLayersBoundaryEditorVM;        
        private FrameworkElement dragReferenceElem;


        private double dragInnerYoffset = 0;
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

        /// <param name="boundIdx">Index of the bound to remove</param>
        private void RemoveLayerBoundary(int boundIdx, LayerSyncronization.FreeSpaceAccepter freeSpaceAccepter)
        {
            allLayersBoundaryEditorVM.RemoveBoundary(boundIdx);
            switch (freeSpaceAccepter)
            {
                case LayerSyncronization.FreeSpaceAccepter.LowerLayer:
                    LayerSyncController.RemoveLayer(boundIdx -1, LayerSyncronization.FreeSpaceAccepter.LowerLayer); break;
                case LayerSyncronization.FreeSpaceAccepter.UpperLayer:
                    LayerSyncController.RemoveLayer(boundIdx, LayerSyncronization.FreeSpaceAccepter.UpperLayer); break;
                default: throw new NotImplementedException();
            }
        }

        private SampleVM sampleUnderCorrection;
        public SampleVM SampleUnderCorrection {
            get {
                return sampleUnderCorrection;
            }
            set {
                if (sampleUnderCorrection != value) {
                    sampleUnderCorrection = value;
                    RaisePropertyChanged(nameof(SampleUnderCorrection));
                }
            }
        }

        public ICommand CloseSampleEditingCommand  { get; private set;}

        /// <param name="boundIdx">which bound to move?</param>
        /// <param name="level">in WPF units</param>
        private void MoveLayerBoundary(int boundIdx, double level)
        {
            allLayersBoundaryEditorVM.MoveBoundary(boundIdx, level);
            double up = 0.0;
            if (boundIdx > 0)
                up = LayerSyncController.GetLayerWPFTop(boundIdx -1);
            LayerSyncController.MoveBoundary(boundIdx-1, level - up);
        }

        private void AddBoundary(double level)
        {
            allLayersBoundaryEditorVM.AddBoundary(0, level);
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
        private readonly Dictionary<string, LayerClassVM[]> availableClasses = new Dictionary<string, LayerClassVM[]>();

        private SamplesColumnVM samplesColumnVM;

        public SamplesColumnVM SamplesColumnVM
        {
            get { return samplesColumnVM; }
        }

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

        private ICommand saveProjectCommand;

        public ICommand SaveProjectCommand {
            get { return saveProjectCommand; }
            set {
                if (saveProjectCommand != value) {
                    saveProjectCommand = value;
                    RaisePropertyChanged(nameof(SaveProjectCommand));
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

        private ICommand zoomInCommand;
        public ICommand ZoomInCommand {
            get {
                return zoomInCommand;
            }
            set {
                if (zoomInCommand != value) {
                    zoomInCommand = value;
                    RaisePropertyChanged(nameof(ZoomInCommand));
                }
            }
        }

        private ICommand zoomOutCommand;
        public ICommand ZoomOutCommand
        {
            get {
                return zoomOutCommand;
            }
            set {
                if (zoomOutCommand != value) {
                    zoomOutCommand = value;
                    RaisePropertyChanged(nameof(ZoomOutCommand));
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
                RecalcAllBoundaries(ColScaleController.UpperDepth, ColScaleController.UpperDepth, ColScaleController.ScaleFactor,value);
                if (value != ColScaleController.ScaleFactor)
                {
                    ColScaleController.ScaleFactor = value;
                    layerSyncController.ScaleFactor = value;
                }
            }
        }

        /// <param name="oldUpDepth">in meters</param>
        /// <param name="newUpDepth">in meters</param>        
        /// <param name="oldScaleFactor">How much is 1 real meter in WPF coordinates</param>
        /// <param name="newScaleFactor">How much is 1 real meter in WPF coordinates></param>
        private void RecalcAllBoundaries(double oldUpDepth, double newUpDepth, double oldScaleFactor, double newScaleFactor) {
            if (allLayersBoundaryEditorVM != null)
            {
                var boundaries = allLayersBoundaryEditorVM.Boundaries;
                double[] depths = boundaries.Select(b => b.Level / oldScaleFactor + oldUpDepth).ToArray();
                double[] newLevels = depths.Select(d => (d - newUpDepth) * newScaleFactor).ToArray();
                LayerBoundary[] newBoundaries = new LayerBoundary[boundaries.Length];
                for (int i = 0; i < boundaries.Length; i++)
                {
                    newBoundaries[i] = new LayerBoundary(newLevels[i], boundaries[i].Rank);
                }
                allLayersBoundaryEditorVM.Boundaries = newBoundaries;
            }
        }

        private readonly ILayersTemplateSource layersTemplateSource;
        private readonly int MaxRank;

        public PlaneVM(LayersAnnotation annotation, ILayersTemplateSource layersTemplateSource, ILayerRankNamesSource rankNames)
        {
            this.layersTemplateSource = layersTemplateSource;            
            MaxRank = rankNames.GeneritiveNames.Length - 1;
            
            double upperDepth = annotation.LayerBoundaries[0];
            double lowerDepth = annotation.LayerBoundaries[annotation.LayerBoundaries.Length - 1];

            ScaleFactor = 3000.0;
            ColScaleController.UpperDepth = upperDepth;
            LayerSyncController.UpperDepth = upperDepth;
            ColScaleController.LowerDepth = lowerDepth;
            LayerSyncController.LowerDepth = lowerDepth;

            double wpfHeight = LayerSyncController.LengthToWPF(lowerDepth-upperDepth);

            this.allLayersBoundaryEditorVM = new LayerBoundaryEditorVM(wpfHeight, MaxRank, AnnotationDirection);
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
            if (cl.Description != null)
                result.Description = cl.Description;
            if (cl.ShortName != null)
                result.ShortName = cl.ShortName;
            if (!double.IsNaN(cl.WidthRatio) && !double.IsInfinity(cl.WidthRatio))
            {
                result.WidthRatio = cl.WidthRatio;
            }
            result.RightSideForm = cl.RightSideForm;
            if (cl.BackgroundPatternSVG != null)
            {
                Svg.SvgPatternServer pa = Svg.SvgDocument.FromSvg<Svg.SvgDocument>(cl.BackgroundPatternSVG).Children[0] as Svg.SvgPatternServer;

                Svg.SvgPatternServer paOrig = (Svg.SvgPatternServer)pa.DeepCopy();

                Svg.SvgColourServer whitePaint = new Svg.SvgColourServer(System.Drawing.Color.White);

                //creating a brush from the pattern
                Svg.SvgDocument doc = new Svg.SvgDocument();
                Svg.SvgDefinitionList defs = new Svg.SvgDefinitionList();
                pa.ID = string.Format("pattern-{0}", Guid.NewGuid().ToString());
                paOrig.ID = pa.ID;
                pa.PatternUnits = Svg.SvgCoordinateUnits.Inherit;

                double maxSize = Math.Max(pa.Width.Value,pa.Height.Value);
                double scale = 0.25 / maxSize;

                pa.Width = new Svg.SvgUnit((float)(pa.Width.Value * scale));
                pa.Height = new Svg.SvgUnit((float)(pa.Height.Value * scale));

                defs.Children.Add(pa);
                doc.Children.Add(defs);

                Svg.SvgRectangle rect = new Svg.SvgRectangle();
                rect.Width = 256;
                rect.Height = 256;
                rect.Fill = pa;
                doc.Children.Add(rect);

                var bitmap = doc.Draw();

                //bitmap.Save("pattern.png");

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                                                            IntPtr.Zero,
                                                                            Int32Rect.Empty,
                                                                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
                                                                         );
                bitmap.Dispose();

                var brush = new ImageBrush(bitmapSource)
                {
                    TileMode = TileMode.Tile,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    ViewportUnits = BrushMappingMode.Absolute
                };

                BindingOperations.SetBinding(brush, ImageBrush.ViewportProperty, new Binding("ImageSource") { Converter = new Columns.ViewPortConverter(), RelativeSource = RelativeSource.Self });

                result.BackgroundBrush = brush;
                result.BackgroundPattern = paOrig;
            }
            if (cl.IconSVG != null)
            {
                Svg.SvgDocument doc = Helpers.SvgFromString(cl.IconSVG);
                Svg.SvgFragment docCopy = (Svg.SvgFragment)doc.DeepCopy();
                doc.Fill = new Svg.SvgColourServer(System.Drawing.Color.White);

                double ratio = doc.Bounds.Width / doc.Bounds.Height;

                int width = 32;
                int height = (int)(width / ratio);

                doc.ViewBox = new Svg.SvgViewBox(0, 0, doc.Bounds.Width, doc.Bounds.Height);

                doc.Width = width;
                doc.Height = height;
                var bitmap = doc.Draw();

                //bitmap.Save(string.Format("{0}-icon.png", cl.ID));

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                                                            IntPtr.Zero,
                                                                            Int32Rect.Empty,
                                                                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
                                                                        );
                bitmap.Dispose();
                result.IconSvg = docCopy;
                result.IconImage = bitmapSource;
            }
            if (cl.ExampleImage != null)
                result.ExampleImage = cl.ExampleImage;
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
                availableClasses.Add(p.ID, availableClassesOfP.ToArray()); //global dict of available choices
            }


            AnnoGridVM = new AnnotationGridVM();
            
            ElementDropped = new DelegateCommand(obj =>
            {
                ElemDroppedEventArgs edea = obj as ElemDroppedEventArgs;
                LayerBoundary boundary = edea.DroppedElement.Tag as LayerBoundary;
                SampleVM sample = edea.DroppedElement.Tag as SampleVM;
                if (boundary != null)
                {
                    System.Diagnostics.Debug.WriteLine("dropped boundary {0} with offset {1} in {2} column", boundary.ID, edea.WpfTopOffset, edea.ColumnIdx);
                    int boundIdx = Array.FindIndex(allLayersBoundaryEditorVM.Boundaries, b => b.ID == boundary.ID);
                    double prevTop = layerSyncController.GetLayerWPFTop(boundIdx-1);
                    double prevBottom = layerSyncController.GetLayerWPFBottom(boundIdx-1);
                    double nextBottom = layerSyncController.GetLayerWPFBottom(boundIdx);
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
                                allLayersBoundaryEditorVM.Boundaries = allLayersBoundaryEditorVM.Boundaries.ToArray(); // this recreates all views, after the boundary label dragging
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
                                allLayersBoundaryEditorVM.Boundaries = allLayersBoundaryEditorVM.Boundaries.ToArray(); // this recreates all views, after the boundary label dragging
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
                                    allLayersBoundaryEditorVM.ChangeRank(boundary.ID, rfbc.Rank);
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
                    else
                    {
                        var res = MessageBox.Show(
                            string.Format("Удалить образец {0} с глубины {1:#.##}м?", sample.Number, sample.Depth),
                            "Подтверждение удаления образца",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        switch (res)
                        {
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

                Type[] newLayerTypes = new Type[] { typeof(ImageColumnVM), typeof(DepthAxisColumnVM), typeof(BoundaryEditorColumnVM), typeof(BoundaryLineColumnVM) };
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

            CloseSampleEditingCommand = new DelegateCommand(() =>
            {
                if (!string.IsNullOrEmpty(SampleUnderCorrection.Comment))
                    samplesColumnVM.RecentSampleComment = SampleUnderCorrection.Comment;
                SampleUnderCorrection = null;
            });

            LayerSyncController.PropertyChanged += LayerSyncController_PropertyChanged;
            ColScaleController.PropertyChanged += ColScaleController_PropertyChanged;

            zoomInCommand = new DelegateCommand(() => {
                ScaleFactor *= 1.1;
            });

            zoomOutCommand = new DelegateCommand(() =>
            {
                ScaleFactor /= 1.1;
            });
            
            classificationVM = new ClassificationVM(new IDEncodedTreeBuilder('@'));
            classificationVM.CloseCommand = new DelegateCommand(() => classificationVM.IsVisible = false);
            classificationVM.ClassSelectedCommand = new DelegateCommand(sender =>
            {
                FrameworkElement fe = sender as FrameworkElement;
                LeafTreeNode ltn = (LeafTreeNode)fe.DataContext;
                LayerClassVM lc = ltn.AssociatedClass;
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
            classificationVM.GroupSelectedCommand = new DelegateCommand(sender => {
                FrameworkElement fe = sender as FrameworkElement;
                NonLeafTreeNodeVM nltn = (NonLeafTreeNodeVM)fe.DataContext;
                classificationVM.CurrentlyObservedNode = nltn;
            });
            classificationVM.IsVisible = false;

            double[] boundaries = annotation.LayerBoundaries;
            int layersCount = boundaries.Length - 1;

            double upperDepth = LayerSyncController.LowerDepth;
            double lowerDepth = LayerSyncController.UpperDepth;
                
            allLayersBoundaryEditorVM.DragStart = new DelegateCommand(args =>
            {
                if (DragReferenceElem != null)
                {
                    DragStartEventArgs dsea = args as DragStartEventArgs;
                    Point startDragCoord = dsea.GetEventPoint(DragReferenceElem);
                    Point localDragOffset = dsea.GetEventPoint(dsea.FrameworkElement);
                    Point dragItemLocation = startDragCoord - new Vector(localDragOffset.X, localDragOffset.Y);
                    AnnoGridVM.LocalDraggedItemOffset = localDragOffset;
                    AnnoGridVM.DragItemInitialLocation = dragItemLocation;
                    dsea.FrameworkElement.Tag = dsea.FrameworkElement.DataContext; // storing original data context in tag befor the element is moved out of the UI tree
                    AnnoGridVM.DragCandidateItem = dsea.FrameworkElement;
                }
            });

            samplesColumnVM.DragStart = allLayersBoundaryEditorVM.DragStart;
            samplesColumnVM.SampleEditRequestedCommand = new DelegateCommand(arg => {
                SampleUnderCorrection = (SampleVM)arg;
            });

            double colHeight = LayerSyncController.DepthToWPF(lowerDepth) - LayerSyncController.DepthToWPF(upperDepth);

            //filling up classification columns (not UI column, logical columns. One column for each property)
            foreach (var property in layersTemplateSource.Template)
            {
                LayeredColumnVM columnVM = new LayeredColumnVM(property.ID);
                for (int i = 0; i < layersCount; i++)
                {
                    double layerUp = boundaries[i];
                    double layerBottom = boundaries[i + 1];

                    ClassificationLayerVM layerVM;
                    if (property.IsMulticlass)
                    {
                        layerVM = new MultiClassificationLayerVM(property.Name);
                    }
                    else
                    {
                        layerVM = new SingleClassificationLayerVM(property.Name);
                    }
                    layerVM.PossibleClasses = availableClasses[property.ID];
                    ColumnValues column = annotation.Columns.FirstOrDefault(c => c.PropID == property.ID);
                    if (column != null)
                    {
                        //setting Remarks
                        if (column.LayerValues[i].Remarks != null)
                        {
                            layerVM.Remark = column.LayerValues[i].Remarks;
                        }
                        //setting choice
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
                    layerVM.Length = LayerSyncController.LengthToWPF(layerBottom - layerUp);
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

            var syncAdapter = new SyncronizerBoundaryAdapter(allLayersBoundaryEditorVM);

            //            LayerSyncController.RegisterColumn(syncAdapter);
            ColScaleController.AttachToColumn(syncAdapter);
        }

        /// <summary>
        /// Including outer boundaries (the level of first layer start and the level of last layer end)
        /// </summary>
        public LayerBoundary[] LayerBoundaries
        {
            get
            {
                return allLayersBoundaryEditorVM.Boundaries;
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

                    ILayerBoundariesVM firstLastOmited = new RemoveFirstDecoratorLBVM(new RemoveLastDecoratorLBVM(allLayersBoundaryEditorVM));                    
                    BoundaryLineColumnVM blVM = new BoundaryLineColumnVM(colVM, firstLastOmited, Colors.Green);

                    blVM.ColumnHeight = colHeight;
                    AnnoGridVM.Columns.Add(blVM);
                    RegisterForScaleSync(blVM, true);
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

                    ILayerBoundariesVM firstLastOmited = new RemoveFirstDecoratorLBVM(new RemoveLastDecoratorLBVM(allLayersBoundaryEditorVM));                    
                    BoundaryLineColumnVM blVM = new BoundaryLineColumnVM(imColVM, firstLastOmited, Colors.Lime);

                    blVM.ColumnHeight = colHeight;
                    imColVM.ImageRegions = photos;
                    AnnoGridVM.Columns.Add(blVM);
                    RegisterForScaleSync(blVM, true);
                }
                else if (columnDefinition is LayeredTextColumnDefinitionVM)
                {
                    LayeredTextColumnDefinitionVM colDef = (LayeredTextColumnDefinitionVM)columnDefinition;
                    if (colDef.SelectedCentreTextProp != null)
                    {
                        string propID = colDef.SelectedCentreTextProp.PropID;

                        LayeredColumnVM propColumnVM = LayerProps.Where(p => p.Heading == propID).FirstOrDefault();
                        if (propColumnVM == null)
                            continue;

                        //Creating presentation column.

                        //here is the actual displayed text is set
                        Func<LayerClassVM, string> centreTextExtractor;
                        switch (colDef.SelectedCentreTextProp.Presentation)
                        {
                            case Presentation.Acronym: centreTextExtractor = vm1 => vm1.Acronym; break;
                            case Presentation.Description: centreTextExtractor = vm1 => vm1.Description; break;
                            case Presentation.ShortName: centreTextExtractor = vm1 => vm1.ShortName; break;
                            case Presentation.BackgroundImage: throw new InvalidOperationException();
                            case Presentation.Icon: throw new InvalidOperationException();
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
                    if (colDef.Selected != null)
                    {
                        int rank = colDef.SelectedIndex;
                        string heading = string.Format("Границы между {0}", colDef.Selected.ToLowerInvariant());
                        BlankColumnVM blankColumnVM = new BlankColumnVM(heading);                        
                        ILayerBoundariesVM lastBoundaryRemoved = new RemoveLastDecoratorLBVM(allLayersBoundaryEditorVM);
                        ILayerBoundariesVM firstAndLastBoundaryRemoved = new RemoveFirstDecoratorLBVM(lastBoundaryRemoved); 
                                               
                        RankMoreOrEqualBoundaryCollection filteredForLabels = new RankMoreOrEqualBoundaryCollection(lastBoundaryRemoved, rank);
                        BoundaryLabelColumnVM blaVM = new BoundaryLabelColumnVM(blankColumnVM, filteredForLabels,rank);
                        RankMoreOrEqualBoundaryCollection filteredForLines = new RankMoreOrEqualBoundaryCollection(firstAndLastBoundaryRemoved, rank);
                        BoundaryLineColumnVM blVM = new BoundaryLineColumnVM(blaVM, filteredForLines, Colors.Black);
                        RankMatchingBoundaryCollection filteredForDraggables = new RankMatchingBoundaryCollection(firstAndLastBoundaryRemoved, rank);
                        BoundaryEditorColumnVM beVM = new BoundaryEditorColumnVM(blVM, filteredForDraggables,rank);


                        blankColumnVM.ColumnHeight = colHeight;
                        blankColumnVM.ColumnWidth = 100;
                        AnnoGridVM.Columns.Add(beVM);
                        RegisterForScaleSync(beVM, true);
                    }
                }
                else if (columnDefinition is LayerSamplesDefinitionVM)
                {
                    AnnoGridVM.Columns.Add(samplesColumnVM);
                }
                else if (columnDefinition is VisualColumnDefinitionVM)
                {
                    VisualColumnDefinitionVM vcdVM = (VisualColumnDefinitionVM)columnDefinition;
                    if (vcdVM.SelectedBackgroundImageProp == null)
                        continue;
                    LayeredColumnVM propColumnVM = LayerProps.Where(p => p.Heading == vcdVM.SelectedBackgroundImageProp.PropID).FirstOrDefault();
                    if (propColumnVM == null)
                        continue;
                    Columns.VisualColumnVM vcVM = new Columns.VisualColumnVM("Колонка", propColumnVM, lVM => new Columns.VisualLayerPresentingVM(lVM));
                    AnnoGridVM.Columns.Add(vcVM);
                    RegisterForScaleSync(vcVM, true);
                }
                else if (columnDefinition is IconsColumnDefinitionVM)
                {
                    IconsColumnDefinitionVM colDef = (IconsColumnDefinitionVM)columnDefinition;
                    if (colDef.SelectedIconProp == null)
                        continue;
                    LayeredColumnVM propColumnVM = LayerProps.Where(p => p.Heading == colDef.SelectedIconProp.PropID).FirstOrDefault();

                    if (propColumnVM == null)
                        continue;

                    LayeredPresentationColumnVM colVM = new LayeredPresentationColumnVM(
                            colDef.SelectedIconProp.TexturalString,
                            propColumnVM,
                            lVM =>
                            {
                                if (lVM is MultiClassificationLayerVM)
                                    return new MultiClassificationLayerIconPresentingVM((MultiClassificationLayerVM)lVM);
                                else throw new InvalidOperationException("Unexpected VM type");
                            });

                    colVM.ColumnHeight = colHeight;

                    AnnoGridVM.Columns.Add(colVM);
                    RegisterForScaleSync(colVM, true);
                }
                else throw new NotSupportedException("Незнакомое определение колонки");
            }
        }


        public LayersAnnotation AsLayersAnnotation
        {
            get
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
                        //handling remarks                    
                        v.Remarks = clVM.Remark;
                        //handling selected values                    
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
                return layersAnnotation;
            }
        }


        #region Persistence

        protected PlaneVM(SerializationInfo info, StreamingContext context)
        {
            LayersAnnotation layersAnnotation = (LayersAnnotation)info.GetValue("LayersAnnotation", typeof(LayersAnnotation));
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplate", typeof(ILayersTemplateSource));

            MaxRank = info.GetInt32("MaxRank");
            double scale = info.GetDouble("Scale");
            double lowerDepth = info.GetDouble("LowerDepth");
            double upperDepth = info.GetDouble("UpperDepth");

            ScaleFactor = scale;
            LayerSyncController.UpperDepth = upperDepth;
            ColScaleController.UpperDepth = upperDepth;
            LayerSyncController.LowerDepth = lowerDepth;
            ColScaleController.LowerDepth = lowerDepth;

            annotationDirection = (AnnotationDirection)info.GetValue("AnnotationDirection", typeof(AnnotationDirection));
            allLayersBoundaryEditorVM = (LayerBoundaryEditorVM)info.GetValue("LayerBoundaries", typeof(LayerBoundaryEditorVM));
            samplesColumnVM = (SamplesColumnVM)info.GetValue("Samples", typeof(SamplesColumnVM));
            
            Initialize(layersAnnotation);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            info.AddValue("Samples", samplesColumnVM);
            info.AddValue("AnnotationDirection", AnnotationDirection);
            info.AddValue("LayersAnnotation", AsLayersAnnotation);
            info.AddValue("LayersTemplate", layersTemplateSource);
            info.AddValue("LayerBoundaries", allLayersBoundaryEditorVM);
            info.AddValue("MaxRank", MaxRank);
            info.AddValue("Scale",ScaleFactor);
            info.AddValue("LowerDepth", LayerSyncController.LowerDepth);
            info.AddValue("UpperDepth", LayerSyncController.UpperDepth);


        }

        #endregion
    }
}
