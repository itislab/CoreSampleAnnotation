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

    public abstract class ClassificationLayerVM : LayerVM
    {


        private IEnumerable<LayerClassVM> possibleClasses;
        public IEnumerable<LayerClassVM> PossibleClasses
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
    }

    public class SingleClassificationLayerVM : ClassificationLayerVM {
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

        public override LayerVM DeepClone()
        {
            SingleClassificationLayerVM result = new SingleClassificationLayerVM();
            result.PossibleClasses = new ObservableCollection<LayerClassVM>(PossibleClasses);
            result.CurrentClass = CurrentClass;
            return result;
        }
    }

    public class MultiClassificationLayerVM : ClassificationLayerVM
    {
        private IEnumerable<LayerClassVM> currentClasses = null;

        public IEnumerable<LayerClassVM> CurrentClasses
        {
            get { return currentClasses; }
            set
            {
                if (currentClasses != value)
                {
                    currentClasses = value;
                    RaisePropertyChanged(nameof(CurrentClasses));
                }
            }
        }

        public override LayerVM DeepClone()
        {
            MultiClassificationLayerVM result = new MultiClassificationLayerVM();
            result.PossibleClasses = new ObservableCollection<LayerClassVM>(PossibleClasses);
            if (CurrentClasses == null)
                result.CurrentClasses = new List<LayerClassVM>();
            else
                result.CurrentClasses = new List<LayerClassVM>(CurrentClasses);
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
            Length = target.Length;
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

    public abstract class ClassificationLayerTextPresentingVM : ClassificationLayerPresentingVM
    {
        /// <summary>
        /// Text digest, representing current state of choice
        /// </summary>
        public string Text
        {
            get
            {
                return TextFormatter();
            }
        }

        /// <summary>
        /// How to extract piece of text from Class VM?
        /// </summary>
        public Func<LayerClassVM, string> TextExtractor { get; set; }

        /// <summary>
        /// How to format Text (depends on internal structure of derived classes)
        /// </summary>
        protected abstract Func<string> TextFormatter { get; }

        public ClassificationLayerTextPresentingVM(ClassificationLayerVM target) : base(target) {
        }
    }

    public class SingleClassificationLayerTextPresentingVM : ClassificationLayerTextPresentingVM {
        private SingleClassificationLayerVM specificTarget;        

        protected override Func<string> TextFormatter
        {
            get {
                if (specificTarget != null)
                    if (specificTarget.CurrentClass != null)
                        return () => TextExtractor(specificTarget.CurrentClass);
                    else
                        return () => null;
                else
                    return () => null;
            }
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SingleClassificationLayerVM vm = sender as SingleClassificationLayerVM;
            switch (e.PropertyName)
            {
                case nameof(vm.CurrentClass):
                    RaisePropertyChanged(nameof(Text));
                    break;
            }
        }

        public SingleClassificationLayerTextPresentingVM(SingleClassificationLayerVM target) : base(target) {
            specificTarget = target;
            target.PropertyChanged += Target_PropertyChanged;
        }
    }    

    public class MultiClassificationLayerTextPresentingVM : ClassificationLayerTextPresentingVM {
        private MultiClassificationLayerVM specificTarget;

        public MultiClassificationLayerTextPresentingVM(MultiClassificationLayerVM target) : base(target) {
            specificTarget = target;
            target.PropertyChanged += Target_PropertyChanged;
        }

        protected override Func<string> TextFormatter {
            get {
                if (specificTarget != null)
                    if (specificTarget.CurrentClasses != null)
                        return () => string.Join(", ",specificTarget.CurrentClasses.Select(c => TextExtractor(c)).ToArray());
                    else
                        return () => null;
                else
                    return () => null;
            }
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MultiClassificationLayerVM vm = sender as MultiClassificationLayerVM;
            switch (e.PropertyName)
            {
                case nameof(vm.CurrentClasses):
                    RaisePropertyChanged(nameof(Text));
                    break;
            }
        }
    }

    public class MultiClassificationLayerIconPresentingVM : ClassificationLayerPresentingVM
    {
        private new MultiClassificationLayerVM target;

        public MultiClassificationLayerIconPresentingVM(MultiClassificationLayerVM target) : base(target) {
            this.target = target;
            target.PropertyChanged += Target_PropertyChanged;
        }
        
        public ImageSource[] Icons {
            get {
                if (target.CurrentClasses == null)
                    return null;
                var results = target.CurrentClasses.Select(c => c.IconImage).ToArray();
                return results;
            }
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MultiClassificationLayerVM vm = sender as MultiClassificationLayerVM;
            switch (e.PropertyName)
            {
                case nameof(vm.CurrentClasses):
                    RaisePropertyChanged(nameof(Icons));
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

        public string VisibleShortTextID {
            get {
                if (string.IsNullOrEmpty(acronym))
                {
                    return ID;
                }
                else
                    return Acronym;       
            }
        }

        public string VisibleLongTextID {
            get {
                if (string.IsNullOrEmpty(Description))
                {
                    return ShortName;
                }
                else
                    return Description;
            }
        }

        private Brush backgroundBrush;        

        public Brush BackgroundBrush {
            get { return backgroundBrush; }
            set {
                if (backgroundBrush != value) {
                    backgroundBrush = value;
                    RaisePropertyChanged(nameof(BackgroundBrush));
                }
            }
        }

        private Svg.SvgPatternServer backgroundPattern;

        public Svg.SvgPatternServer BackgroundPattern {
            get { return backgroundPattern; }
            set {
                if (backgroundPattern != value) {
                    backgroundPattern = value;
                    RaisePropertyChanged(nameof(BackgroundPattern));
                }
            }
        }

        private ImageSource iconImage;

        public ImageSource IconImage {
            get {
                return iconImage;
            }
            set {
                if (iconImage != value) {
                    iconImage = value;
                    RaisePropertyChanged(nameof(IconImage));
                }
            }
        }

        private Svg.SvgElement iconSvg;
        public Svg.SvgElement IconSvg {
            get { return IconSvg; }
            set {
                if (iconSvg != value) {
                    iconSvg = value;
                    RaisePropertyChanged(nameof(IconSvg));
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

