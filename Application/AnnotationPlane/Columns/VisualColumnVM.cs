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
        protected SingleClassificationLayerVM target;

        public SingleClassificationLayerVM Origin {
            get {
                return target;
            }
        }

        public VisualLayerPresentingVM(SingleClassificationLayerVM target)
        {
            this.target = target;
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(target.CurrentClass):
                    RaisePropertyChanged(nameof(BackgroundBrush));
                    RaisePropertyChanged(nameof(Width));
                    break;
                case nameof(target.Length):
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
            get { return target.Length; }
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

        public double WidthRatio {
            get {
                if (target.CurrentClass == null)
                    return 1.0;
                else
                    return target.CurrentClass.WidthRatio;
            }

        }

        public double Width {
            get {
                return AvailableWidth * WidthRatio;
            }
        }

        public Template.RightSideFormEnum RightSideForm {
            get {
                if (target.CurrentClass == null)
                    return Template.RightSideFormEnum.Straight;
                else
                    return target.CurrentClass.RightSideForm;
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                if (target.CurrentClass == null || target.CurrentClass.BackgroundBrush == null)
                    return null;
                else
                    return target.CurrentClass.BackgroundBrush;
            }
        }
    }

    public class VisualColumnVM : ColumnVM
    {
        private LayeredColumnVM target;
        private Func<SingleClassificationLayerVM, VisualLayerPresentingVM> adapter;

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

        public VisualColumnVM(string heading, LayeredColumnVM lVm, Func<SingleClassificationLayerVM, VisualLayerPresentingVM> adapter) : base(heading)
        {
            this.adapter = adapter;
            this.target = lVm;
            target.Layers.CollectionChanged += Layers_CollectionChanged;
            target.PropertyChanged += Target_PropertyChanged;
            columnWidth = 200.0;

            foreach (var layerVM in target.Layers)            
                layerVM.PropertyChanged += SingleClass_Vm_PropertyChanged;

            Reinit();
        }

        /// <summary>
        /// Repopulates layers
        /// </summary>
        private void Reinit() {
            List<VisualLayerPresentingVM> content = new List<VisualLayerPresentingVM>();

            foreach (SingleClassificationLayerVM vm in target.Layers)
            {                
                var adapted = adapter(vm);
                adapted.AvailableWidth = ColumnWidth;
                content.Add(adapted);
            }

            Layers = new ObservableCollection<VisualLayerPresentingVM>(content);
            ReclacYs();
        }

        private void SingleClass_Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Reinit();
        }

        private void ReclacYs()
        {
            SingleClassificationLayerVM[] layers = target.Layers.Select(l => (SingleClassificationLayerVM)l).ToArray();
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
                case nameof(target.LowerBound):
                    LowerBound = target.LowerBound;
                    break;
                case nameof(target.UpperBound):
                    UpperBound = target.UpperBound;
                    break;
                case nameof(target.ColumnHeight):
                    ColumnHeight = target.ColumnHeight;
                    break;
            }
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        SingleClassificationLayerVM addedVM = (SingleClassificationLayerVM)e.NewItems[i];
                        addedVM.PropertyChanged += SingleClass_Vm_PropertyChanged;
                        VisualLayerPresentingVM craftedVM = adapter(addedVM);
                        craftedVM.AvailableWidth = columnWidth;
                        if (e.NewItems.Count == 0)
                            Layers.Add(craftedVM);
                        else
                            Layers.Insert(e.NewStartingIndex + i, craftedVM);
                    }
                    ReclacYs();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count != 1)
                        throw new InvalidOperationException();
                    SingleClassificationLayerVM removedVM = (SingleClassificationLayerVM)e.OldItems[0];
                    removedVM.PropertyChanged -= SingleClass_Vm_PropertyChanged;
                    Layers.RemoveAt(e.OldStartingIndex);
                    ReclacYs();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
