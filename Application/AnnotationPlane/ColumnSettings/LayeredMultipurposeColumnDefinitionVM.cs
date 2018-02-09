using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    public enum Presentation { Colour, Acronym, ShortName, Description }

    public class Variant {
        public string PropertyName { get; private set; }
        public Presentation Presentation { get; private set; }

        public Variant(string propName, Presentation presentation) {
            PropertyName = propName;
            Presentation = presentation;
        }

        public string TexturalString {
            get {
                return string.Format("{0} - {1}", PropertyName, Presentation.ToString());
            }
        }
    }

    public class LayeredMultipurposeColumnDefinitionVM : ColumnDefinitionVM
    {
        private Variant selectedBackgroundColorProp;
        public Variant SelectedBackgroundColorProp {
            get { return selectedBackgroundColorProp; }
            set {
                if (selectedBackgroundColorProp != value) {
                    selectedBackgroundColorProp = value;
                    RaisePropertyChanged(nameof(SelectedBackgroundColorProp));
                }
            }
        }

        private Variant[] availableBackgroundColorProps;
        public Variant[] AvailableBackgroundColorProps {
            get { return availableBackgroundColorProps; }
            set {
                if (availableBackgroundColorProps != value) {
                    availableBackgroundColorProps = value;
                    RaisePropertyChanged(nameof(AvailableBackgroundColorProps));
                }
            }
        }

        private Variant selectedCentreTextProp;
        public Variant SelectedCentreTextProp
        {
            get { return selectedBackgroundColorProp; }
            set
            {
                if (selectedCentreTextProp != value)
                {
                    selectedCentreTextProp = value;
                    RaisePropertyChanged(nameof(SelectedCentreTextProp));
                }
            }
        }

        private Variant[] availableCentreTextProps;
        public Variant[] AvailableCentreTextProps
        {
            get { return availableCentreTextProps; }
            set
            {
                if (availableCentreTextProps != value)
                {
                    availableCentreTextProps = value;
                    RaisePropertyChanged(nameof(AvailableCentreTextProps));
                }
            }
        }
    }
}
