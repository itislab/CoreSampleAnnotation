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

    public class ClassificationLayerPresentingVM : LayerVM {

        protected ClassificationLayerVM target;

        public ClassificationLayerVM ClassificationVM
        {
            get { return target; }
        }

        public ClassificationLayerPresentingVM(ClassificationLayerVM target)
        {
            this.target = target;
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClassificationLayerVM vm = sender as ClassificationLayerVM;
            switch (e.PropertyName)
            {                
                case nameof(vm.Length):
                    Length = vm.Length;
                    RaisePropertyChanged(nameof(Length));
                    break;
            }
        }
    }

    public class ClassificationLayerTextPresentingVM : ClassificationLayerPresentingVM
    {
        public Func<LayerClassVM, string> TextExtractor { get; set; }
        public string Text
        {
            get
            {
                if (target != null)
                    if (target.CurrentClass != null)
                        return TextExtractor(target.CurrentClass);
                    else
                        return null;
                else
                    return null;
            }
        }

        public ClassificationLayerTextPresentingVM(ClassificationLayerVM target):base(target) {
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClassificationLayerVM vm = sender as ClassificationLayerVM;
            switch (e.PropertyName)
            {
                case nameof(vm.CurrentClass):                    
                    RaisePropertyChanged(nameof(Text));
                    break;
            }
        }

    }

    public class LayerClassVM : ViewModel
    {
        public string ID { private set; get; }

        public LayerClassVM(string id)
        {
            this.ID = id;
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

