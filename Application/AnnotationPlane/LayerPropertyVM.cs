using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class LayerVM : ViewModel
    {
        private double length = 0;
        /// <summary>
        /// Length of the layer in WPF units
        /// </summary>
        public double Length
        {
            get
            {
                return length;
            }
            set
            {
                if (length != value)
                {
                    length = value;
                    RaisePropertyChanged(nameof(Length));
                }
            }
        }

        public virtual LayerVM DeepClone()
        {
            var result = new LayerVM();
            result.Length = Length;
            return result;
        }
    }

    public class LengthLayerVM : LayerVM
    {
        private double realLength = 0.0;

        /// <summary>
        /// In meters
        /// </summary>
        public double RealLength
        {
            get
            {
                return realLength;
            }
            set
            {
                if (realLength != value)
                {
                    realLength = value;
                    RaisePropertyChanged(nameof(RealLength));
                }
            }
        }

        public override LayerVM DeepClone()
        {
            var result = new LengthLayerVM();
            result.Length = Length;
            result.RealLength = RealLength;
            return result;

        }
    }

    public class IconLayerVM : LayerVM
    {
        public override LayerVM DeepClone()
        {
            var result = new IconLayerVM();
            result.Length = Length;
            return result;
        }
    }

    public class ClassificationLayerVM : LayerVM
    {
        private LayerClassVM currentClass = null;

        public LayerClassVM CurrentClass
        {
            get { return currentClass; }
            set
            {
                if (currentClass != value)
                {
                    currentClass = value;
                    RaisePropertyChanged(nameof(CurrentClass));
                }
            }
        }

        private ObservableCollection<LayerClassVM> possibleClasses = new ObservableCollection<LayerClassVM>();
        public ObservableCollection<LayerClassVM> PossibleClasses
        {
            get { return possibleClasses; }
            set
            {
                if (possibleClasses != value)
                {
                    possibleClasses = value;
                    RaisePropertyChanged(nameof(PossibleClasses));
                }
            }
        }

        public override LayerVM DeepClone()
        {
            var result = new ClassificationLayerVM();
            result.CurrentClass = CurrentClass;
            result.PossibleClasses = new ObservableCollection<LayerClassVM>(PossibleClasses);
            return result;
        }
    }

    public class LayerClassVM : ViewModel
    {
        public string ID { private set; get; }

        public LayerClassVM(string id)
        {
            this.ID = id;
        }

        public Func<Color> BackgroundColorExtractor { get; set; }
        public Color BackgroundColor
        {
            get
            {
                return BackgroundColorExtractor();
            }
        }

        public Func<LayerClassVM, string> CenterTextExtractor { get; set; }
        public string CenterText
        {
            get
            {
                return CenterTextExtractor(this);
            }
        }

        private string acronym;
        public string Acronym
        {
            get { return acronym; }
            set
            {
                if (acronym != value)
                {
                    acronym = value;
                    RaisePropertyChanged(nameof(Acronym));
                }
            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    RaisePropertyChanged(nameof(Description));
                }
            }
        }

        private string shortName;
        public string ShortName
        {
            get { return shortName; }
            set
            {
                if (shortName != value)
                {
                    shortName = value;
                    RaisePropertyChanged(nameof(ShortName));
                }
            }
        }

        private Color color;
        public Color Color
        {
            get { return color; }
            set
            {
                if (color != value)
                {
                    color = value;
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }
    }
}

