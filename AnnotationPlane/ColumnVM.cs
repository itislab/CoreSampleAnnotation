using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationPlane
{
    public abstract class ColumnVM : ViewModel {
        private string heading = "Заголовок";
        public string Heading {
            get {
                return heading;
            }
            set {
                if (heading != value) {
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

        public ColumnVM(string heading) {
            this.heading = heading;
        }

        
    }

    public class DepthAxisColumnVM : ColumnVM {
        private double upperBound = 0;
        /// <summary>
        /// Units: meters
        /// </summary>
        public double UpperBound {
            get {
                return upperBound;
            }
            set {
                if (upperBound != value) {
                    upperBound = value;
                    RaisePropertyChanged(nameof(UpperBound));
                    RaisePropertyChanged(nameof(Range));
                }
            }
        }

        private double lowerBound = 0;
        /// <summary>
        /// Units: meters
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
                    RaisePropertyChanged(nameof(Range));
                }
            }
        }

        public InteractiveDataDisplay.WPF.Range Range {
            get {
                return new InteractiveDataDisplay.WPF.Range(LowerBound, UpperBound);
            }
        }

        public DepthAxisColumnVM(string heading): base(heading){
        }

        public DepthAxisColumnVM(string heading, double lowerBound, double upperBound):this(heading) {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }
    }

    public class LayeredColumnVM: ColumnVM
    {
        private ObservableCollection<LayerVM> layers = new ObservableCollection<LayerVM>();

        public ObservableCollection<LayerVM> Layers {
            get {
                return layers;
            }
            set {
                if (layers != value) {
                    layers = value;
                    RaisePropertyChanged(nameof(Layers));
                }
            }
        }

        public LayeredColumnVM(string heading) : base(heading) { }
    }
}
