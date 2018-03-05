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

        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    RaisePropertyChanged(nameof(Width));
                }
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

        private void Subscribe(VisualLayerPresentingVM vlpVM, SingleClassificationLayerVM sclVM)
        {
            vlpVM.Width = ColumnWidth;
        }


        public VisualColumnVM(string heading, LayeredColumnVM lVm, Func<SingleClassificationLayerVM, VisualLayerPresentingVM> adapter) : base(heading)
        {
            this.adapter = adapter;
            this.target = lVm;
            target.Layers.CollectionChanged += Layers_CollectionChanged;
            target.PropertyChanged += Target_PropertyChanged;
            columnWidth = 80.0;
            
            foreach (SingleClassificationLayerVM vm in target.Layers)
            {
                vm.PropertyChanged += Vm_PropertyChanged;
                var adapted = adapter(vm);
                Subscribe(adapted, vm);
                layers.Add(adapted);
            }
            ReclacYs();
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(SingleClassificationLayerVM.Length):
                    ReclacYs();
                    break;
            }
            ReclacYs();
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
                        VisualLayerPresentingVM craftedVM = adapter(addedVM);
                        Subscribe(craftedVM, addedVM);
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
                    Layers.RemoveAt(e.OldStartingIndex);
                    ReclacYs();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
