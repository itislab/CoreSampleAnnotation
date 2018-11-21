using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane.Columns
{
    public class VisualLayerPresentingVM : ViewModel
    {
        protected readonly SingleClassificationLayerVM backgroundTarget;
        protected readonly SingleClassificationLayerVM widthTarget;
        protected readonly SingleClassificationLayerVM rightSideTarget;
        protected readonly SingleClassificationLayerVM bottomSideTarget;

        public SingleClassificationLayerVM BackgroundClass
        {
            get
            {
                return backgroundTarget;
            }
        }
        public SingleClassificationLayerVM WidthClass
        {
            get
            {
                return widthTarget;
            }
        }
        public SingleClassificationLayerVM RightSideClass
        {
            get
            {
                return rightSideTarget;
            }
        }
        public SingleClassificationLayerVM BottomSideClass
        {
            get
            {
                return bottomSideTarget;
            }
        }

        public VisualLayerPresentingVM(
            SingleClassificationLayerVM backgroundTarget,
            SingleClassificationLayerVM widthTarget,
            SingleClassificationLayerVM rightSideTarget,
            SingleClassificationLayerVM bottomSideTarget)
        {
            this.backgroundTarget = backgroundTarget;
            this.widthTarget = widthTarget;
            this.rightSideTarget = rightSideTarget;
            this.bottomSideTarget = bottomSideTarget;

            backgroundTarget.PropertyChanged += Target_PropertyChanged;

            if (widthTarget != null)
                widthTarget.PropertyChanged += Target_PropertyChanged;

            if (rightSideTarget != null)
                rightSideTarget.PropertyChanged += Target_PropertyChanged;
            
            if (bottomSideTarget != null)
                bottomSideTarget.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(backgroundTarget.CurrentClass):
                    RaisePropertyChanged(nameof(BackgroundBrush));
                    RaisePropertyChanged(nameof(Width));
                    RaisePropertyChanged(nameof(RightSideClass));
                    RaisePropertyChanged(nameof(BottomSideClass));
                    break;
                case nameof(backgroundTarget.Length):
                    RaisePropertyChanged(nameof(Height));
                    break;
            }
        }

        private double y;
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                if (y != value)
                {
                    y = value;
                    RaisePropertyChanged(nameof(Y));
                }
            }
        }

        public double Height
        {
            get { return backgroundTarget.Length; }
        }

        private double availableWidth;
        /// <summary>
        /// How much width (in WPF) can the layer presentation occupy
        /// </summary>
        public double AvailableWidth
        {
            get { return availableWidth; }
            set
            {
                if (availableWidth != value)
                {
                    availableWidth = value;
                    RaisePropertyChanged(nameof(AvailableWidth));
                    RaisePropertyChanged(nameof(Width));
                }
            }
        }

        public double WidthRatio
        {
            get
            {
                if (widthTarget == null || widthTarget.CurrentClass == null)
                    return 1.0;
                else
                {
                    if (double.IsNaN(widthTarget.CurrentClass.WidthRatio))
                        return 1.0;
                    else
                        return widthTarget.CurrentClass.WidthRatio;
                }
            }

        }

        public double Width
        {
            get
            {
                return AvailableWidth * WidthRatio;
            }
        }

        public Template.RightSideFormEnum RightSideForm
        {
            get
            {
                if (RightSideClass == null || RightSideClass.CurrentClass == null)
                    return Template.RightSideFormEnum.Straight;
                else
                    return RightSideClass.CurrentClass.RightSideForm;
            }
        }

        public Template.BottomSideFormEnum BottomSideForm
        {
            get
            {
                if (BottomSideClass == null || BottomSideClass.CurrentClass == null)
                    return Template.BottomSideFormEnum.Straight;
                else
                    return BottomSideClass.CurrentClass.BottomSideForm;
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                if (backgroundTarget.CurrentClass == null || backgroundTarget.CurrentClass.BackgroundBrush == null)
                    return null;
                else
                    return backgroundTarget.CurrentClass.BackgroundBrush;
            }
        }
    }

    public class VisualColumnVM : ColumnVM
    {
        private LayeredColumnVM backgroundTarget;
        private LayeredColumnVM widthTarget;
        private LayeredColumnVM rightSideTarget;
        private LayeredColumnVM bottomSideTarget;

        private Func<SingleClassificationLayerVM, SingleClassificationLayerVM, SingleClassificationLayerVM, SingleClassificationLayerVM, VisualLayerPresentingVM> adapter;

        private ObservableCollection<VisualLayerPresentingVM> layers = new ObservableCollection<VisualLayerPresentingVM>();

        public ObservableCollection<VisualLayerPresentingVM> Layers
        {
            get
            {
                return layers;
            }
            set
            {
                if (layers != value)
                {
                    layers = value;
                    RaisePropertyChanged(nameof(Layers));
                }
            }
        }

        private double columnWidth;
        public double ColumnWidth
        {
            get
            {
                return columnWidth;
            }
            set
            {
                if (columnWidth != value)
                {
                    columnWidth = value;
                    RaisePropertyChanged(nameof(ColumnWidth));
                }
            }
        }

        public VisualColumnVM(string heading,
            LayeredColumnVM backgroundVm,
            LayeredColumnVM widthVm,
            LayeredColumnVM rightSideVm,
            LayeredColumnVM bottomSideVm,
            Func<SingleClassificationLayerVM, SingleClassificationLayerVM, SingleClassificationLayerVM, SingleClassificationLayerVM, VisualLayerPresentingVM> adapter) : base(heading)
        {
            this.adapter = adapter;

            backgroundTarget = backgroundVm;
            widthTarget = widthVm;
            rightSideTarget = rightSideVm;
            bottomSideTarget = bottomSideVm;

            backgroundTarget.Layers.CollectionChanged += Layers_CollectionChanged;
            backgroundTarget.PropertyChanged += Target_PropertyChanged;
            foreach (var layerVM in backgroundTarget.Layers)
                layerVM.PropertyChanged += SingleClass_Vm_PropertyChanged;
            if (widthTarget != null)
            {
                widthTarget.Layers.CollectionChanged += Layers_CollectionChanged;
                widthTarget.PropertyChanged += Target_PropertyChanged;
                foreach (var layerVM in widthTarget.Layers)
                    layerVM.PropertyChanged += SingleClass_Vm_PropertyChanged;
            }
            if (rightSideTarget != null)
            {
                rightSideTarget.Layers.CollectionChanged += Layers_CollectionChanged;
                rightSideTarget.PropertyChanged += Target_PropertyChanged;
                foreach (var layerVM in rightSideTarget.Layers)
                    layerVM.PropertyChanged += SingleClass_Vm_PropertyChanged;
            }
            if (bottomSideTarget != null)
            {
                bottomSideTarget.Layers.CollectionChanged += Layers_CollectionChanged;
                bottomSideTarget.PropertyChanged += Target_PropertyChanged;
                foreach (var layerVM in bottomSideTarget.Layers)
                    layerVM.PropertyChanged += SingleClass_Vm_PropertyChanged;
            }
            columnWidth = 200.0;
            if (widthTarget == null && rightSideTarget == null)
                columnWidth = 60.0;

            Reinit();
        }

        /// <summary>
        /// Repopulates layers
        /// </summary>
        private void Reinit()
        {            
            int N = backgroundTarget.Layers.Count;

            // continuous reiniting during adding of new layers causes cases
            // where some of the underlaying logical prop cols are already splited, while others are not
            // e.g. background image bound already contains N layers, while SideColumn bound yet contains N-1
            // eventually they will have N layers in each logical column
            // but while the layers count is different, just dropping the reinit
            if (widthTarget != null && widthTarget.Layers.Count != N)
                return;
            if (rightSideTarget != null && rightSideTarget.Layers.Count != N)
                return;
            if (bottomSideTarget != null && bottomSideTarget.Layers.Count != N)
                return;

            List<VisualLayerPresentingVM> content = new List<VisualLayerPresentingVM>();
            
            for (int i = 0; i < N; i++)
            {
                SingleClassificationLayerVM background = backgroundTarget.Layers[i] as SingleClassificationLayerVM;
                SingleClassificationLayerVM width;
                if (widthTarget != null)
                    width = widthTarget.Layers[i] as SingleClassificationLayerVM;
                else
                    width = null; //the width source is not chosen
                SingleClassificationLayerVM rightSide;
                if (rightSideTarget != null)
                    rightSide = rightSideTarget.Layers[i] as SingleClassificationLayerVM;
                else
                    rightSide = null; // the right side source is not chosen
                SingleClassificationLayerVM bottomSide;
                if (bottomSideTarget != null)
                    bottomSide = bottomSideTarget.Layers[i] as SingleClassificationLayerVM;
                else
                    bottomSide = null; // the bottom side source is not chosen
                var adapted = adapter(background, width, rightSide, bottomSide);
                adapted.AvailableWidth = ColumnWidth;
                content.Add(adapted);
            }

            Layers = new ObservableCollection<VisualLayerPresentingVM>(content);

            StringBuilder heading = new StringBuilder("");

            heading.Append(string.Format("Колонка\n\nКрап: {0}", backgroundTarget.Heading));
            if (widthTarget != null)
                heading.Append(string.Format("\nШирина: {0}", widthTarget.Heading));
            if (rightSideTarget != null)
                heading.Append(string.Format("\nПравая граница: {0}", rightSideTarget.Heading));
            if (bottomSideTarget != null)
                heading.Append(string.Format("\nНижняя граница: {0}", bottomSideTarget.Heading));

            Heading = heading.ToString();

            ReclacYs();
        }

        private void SingleClass_Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Reinit();
        }

        private void ReclacYs()
        {
            SingleClassificationLayerVM[] layers = backgroundTarget.Layers.Select(l => (SingleClassificationLayerVM)l).ToArray();
            VisualLayerPresentingVM[] vLayer = Layers.ToArray();
            int length = layers.Length;
            double y = 0;
            for (int i = 0; i < length; i++)
            {
                SingleClassificationLayerVM vm = layers[i];
                VisualLayerPresentingVM vvm = vLayer[i];
                vvm.Y = y;
                y += vm.Length;
            }
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(backgroundTarget.LowerBound):
                    LowerBound = backgroundTarget.LowerBound;
                    break;
                case nameof(backgroundTarget.UpperBound):
                    UpperBound = backgroundTarget.UpperBound;
                    break;
                case nameof(backgroundTarget.ColumnHeight):
                    ColumnHeight = backgroundTarget.ColumnHeight;
                    break;
            }
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (LayerVM vm in e.OldItems)
                {
                    vm.PropertyChanged -= SingleClass_Vm_PropertyChanged;
                }
            if (e.NewItems != null)
                foreach (LayerVM vm in e.NewItems)
                {
                    vm.PropertyChanged += SingleClass_Vm_PropertyChanged;
                }
            Reinit();
        }
    }
}
