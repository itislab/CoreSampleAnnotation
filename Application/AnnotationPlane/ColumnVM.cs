using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public abstract class ColumnVM : ViewModel
    {
        private string heading = "Заголовок";
        public string Heading
        {
            get
            {
                return heading;
            }
            set
            {
                if (heading != value)
                {
                    heading = value;
                    RaisePropertyChanged(nameof(Heading));
                }
            }
        }

        private double height = 0.0;
        /// <summary>
        /// In WPF units
        /// </summary>
        public double ColumnHeight
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    height = value;
                    RaisePropertyChanged(nameof(ColumnHeight));
                }
            }
        }

        private double upperBound = 0;
        /// <summary>
        /// Units: meters (positive value)
        /// </summary>
        public double UpperBound
        {
            get
            {
                return upperBound;
            }
            set
            {
                if (upperBound != value)
                {
                    upperBound = value;
                    RaisePropertyChanged(nameof(UpperBound));
                }
            }
        }

        private double lowerBound = 0;
        /// <summary>
        /// Units: meters (positive value)
        /// </summary>
        public double LowerBound
        {
            get
            {
                return lowerBound;
            }
            set
            {
                if (lowerBound != value)
                {
                    lowerBound = value;
                    RaisePropertyChanged(nameof(LowerBound));
                }
            }
        }


        public ColumnVM(string heading)
        {
            this.heading = heading;
        }


    }

    public class DepthAxisColumnVM : ColumnVM
    {

        public InteractiveDataDisplay.WPF.Range Range
        {
            get
            {
                return new InteractiveDataDisplay.WPF.Range(-LowerBound, -UpperBound);
            }
        }

        public DepthAxisColumnVM(string heading) : base(heading)
        {
            this.PropertyChanged += DepthAxisColumnVM_PropertyChanged;
        }

        private void DepthAxisColumnVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(UpperBound)) || (e.PropertyName == nameof(LowerBound)))
            {
                DepthAxisColumnVM dacVM = (DepthAxisColumnVM)sender;
                dacVM.RaisePropertyChanged(nameof(Range));
            }
        }
    }

    public class LayeredColumnVM : ColumnVM
    {
        private ObservableCollection<LayerVM> layers = new ObservableCollection<LayerVM>();

        public ObservableCollection<LayerVM> Layers
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

        public LayeredColumnVM(string heading) : base(heading) { }
    }

    public class ImageColumnVM : ColumnVM
    {
        public ImageColumnVM(string heading) : base(heading)
        {

        }

        /// <summary>
        /// the wpf width of the widest image among <seealso cref="ImageRegions"/>
        /// </summary>
        public double MaxImageWidth {
            get {
                if (ImageRegions != null)
                {
                    var regions = ImageRegions.ToArray();
                    if (regions.Length > 0)
                    {
                        return regions.Max(r => r.WpfWidth);
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        private IEnumerable<Intervals.PhotoRegion> imageRegions = null;
        public IEnumerable<Intervals.PhotoRegion> ImageRegions
        {
            get
            {
                return imageRegions;
            }
            set
            {
                if (imageRegions != value)
                {
                    if (imageRegions != null)
                    {
                        foreach (Intervals.PhotoRegion region in imageRegions)
                        {
                            region.PropertyChanged -= Region_PropertyChanged;
                        }
                    }

                    imageRegions = value;
                    if (imageRegions != null)
                    {
                        foreach (Intervals.PhotoRegion region in imageRegions)
                        {
                            region.PropertyChanged += Region_PropertyChanged;
                        }
                    }
                    RaisePropertyChanged(nameof(ImageRegions));
                    RaisePropertyChanged(nameof(MaxImageWidth));
                }
            }
        }

        private void Region_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Intervals.PhotoRegion region = sender as Intervals.PhotoRegion;
            switch (e.PropertyName) {
                case nameof(region.WpfWidth):
                    RaisePropertyChanged(nameof(MaxImageWidth));
                    break;
            }
        }
    }

    /// <summary>
    /// This is a spectial case of LayeredColumnVM which maintains the up-to-date RealLength property in every LengthLayerVM among layers
    /// </summary>
    public class LayerRealLengthColumnVM : LayeredColumnVM
    {
        public LayerRealLengthColumnVM(string heading) : base(heading)
        {
            this.PropertyChanged += LayerRealSizeColumnVM_PropertyChanged;
            Layers.CollectionChanged += Layers_CollectionChanged;
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateRealLength();
        }

        private void LayerRealSizeColumnVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UpperBound):
                case nameof(LowerBound):
                case nameof(ColumnHeight):
                    UpdateRealLength();
                    break;
            }
        }

        private void UpdateRealLength()
        {
            double realToWpfFactor = (LowerBound - UpperBound) / ColumnHeight;
            for (int i = 0; i < Layers.Count; i++)
            {
                LengthLayerVM llvm = Layers[i] as LengthLayerVM;
                if (llvm != null)
                {
                    llvm.RealLength = llvm.Length * realToWpfFactor;
                }
            }
        }


    }
}
