using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    

    [Serializable]
    public class LayeredTextColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        private ILayersTemplateSource layersTemplateSource;

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

        public LayeredTextColumnDefinitionVM(ILayersTemplateSource layersTemplateSource)
        {
            this.layersTemplateSource = layersTemplateSource;
            Initialize();
        }

        private void Initialize()
        {
            List<Variant> textVariants = new List<Variant>();

            foreach (Property p in layersTemplateSource.Template)
            {
                bool foundDescription = false;
                bool foundAcronym = false;
                bool foundShortName = false;
                foreach (Class c in p.Classes)
                {
                    if (!foundDescription && !(string.IsNullOrEmpty(c.Description)))
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.Description));
                        foundDescription = true;
                    }
                    if (!foundAcronym && !(string.IsNullOrEmpty(c.Acronym)))
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.Acronym));
                        foundAcronym = true;
                    }
                    if (!foundShortName && !(string.IsNullOrEmpty(c.ShortName)))
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.ShortName));
                        foundShortName = true;
                    }
                }
            }

            AvailableCentreTextProps = textVariants.ToArray();
        }

        #region Serialization
        protected LayeredTextColumnDefinitionVM(SerializationInfo info, StreamingContext context) : base(info, context) {
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource", typeof(ILayersTemplateSource));
            Initialize();
            string selectePropID = info.GetString("SelectedPropID");
            if (selectePropID != null) {
                //setting the user choice but only if it is avaialabe in loaded template
                Presentation presentationToSet = (Presentation)info.GetValue("SelectedPresentation", typeof(Presentation));

                selectedCentreTextProp = AvailableCentreTextProps.FirstOrDefault(v => (v.PropID == selectePropID) && (v.Presentation == presentationToSet));
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LayersTemplateSource", layersTemplateSource);
            info.AddValue("SelectedPropID", (selectedCentreTextProp == null) ? (null) : (selectedCentreTextProp.PropID));
            info.AddValue("SelectedPresentation", (selectedCentreTextProp == null) ? (Presentation.Acronym) : (selectedCentreTextProp.Presentation));
        }
        #endregion
    }
}
