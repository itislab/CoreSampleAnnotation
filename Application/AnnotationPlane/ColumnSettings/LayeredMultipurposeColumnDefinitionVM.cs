using CoreSampleAnnotation.AnnotationPlane.Template;
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
        public string PropID { get; private set; }
        public string PropertyName { get; private set; }
        public Presentation Presentation { get; private set; }

        public Variant(string propID, string propName, Presentation presentation) {
            PropID = propID;
            PropertyName = propName;
            Presentation = presentation;
        }

        public string TexturalString {
            get {
                string enumText;
                switch (Presentation) {
                    case Presentation.Acronym:
                        enumText = "Текстовое сокращение";break;
                    case Presentation.Colour:
                        enumText = "Цвет"; break;
                    case Presentation.Description:
                        enumText = "Расширеное текстовое описание"; break;
                    case Presentation.ShortName:
                        enumText = "Короткое имя"; break;
                    default:
                        throw new NotImplementedException();
                }

                return string.Format("{0} свойства \"{1}\"",enumText, PropertyName);
            }
        }
    }

    public class LayeredTextColumnDefinitionVM : ColumnDefinitionVM
    {        
        private Variant selectedCentreTextProp;
        public Variant SelectedCentreTextProp
        {
            get { return selectedCentreTextProp; }
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

        public LayeredTextColumnDefinitionVM(Property[] template)
        {
            List<Variant> textVariants = new List<Variant>();

            foreach (Property p in template)
            {
                bool foundDescription = false;
                bool foundAcronym = false;
                bool foundShortName = false;
                foreach (Class c in p.Classes)
                {                    
                    if (!foundDescription && c.Description != null)
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.Description));
                        foundDescription = true;
                    }
                    if (!foundAcronym && c.Acronym != null)
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.Acronym));
                        foundAcronym = true;
                    }
                    if (!foundShortName && c.ShortName != null)
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.ShortName));
                        foundShortName = true;
                    }
                }
            }
            
            AvailableCentreTextProps = textVariants.ToArray();
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
            get { return selectedCentreTextProp; }
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

        public LayeredMultipurposeColumnDefinitionVM(Property[] template) {
            List<Variant> colourVariants = new List<Variant>();
            List<Variant> textVariants = new List<Variant>();

            foreach (Property p in template) {
                bool foundColour = false;
                bool foundDescription = false;
                bool foundAcronym = false;
                bool foundShortName = false;
                foreach (Class c in p.Classes) {
                    if (!foundColour && c.Color.HasValue)
                    {
                        colourVariants.Add(new Variant(p.ID, p.Name, Presentation.Colour));
                        foundColour = true;                        
                    }
                    if (!foundDescription && c.Description != null)
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.Description));
                        foundDescription = true;
                    }
                    if (!foundAcronym && c.Acronym != null)
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.Acronym));
                        foundAcronym = true;
                    }
                    if (!foundShortName && c.ShortName != null)
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.ShortName));
                        foundShortName = true;
                    }
                }
            }

            AvailableBackgroundColorProps = colourVariants.ToArray();
            AvailableCentreTextProps = textVariants.ToArray();
        }
    }
}
