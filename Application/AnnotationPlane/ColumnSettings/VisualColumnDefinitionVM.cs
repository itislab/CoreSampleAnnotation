using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{

    [Serializable]
    public class VisualColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        private ILayersTemplateSource layersTemplateSource;

        private Variant selectedBackgroundImageProp;
        public Variant SelectedBackgroundImageProp
        {
            get { return selectedBackgroundImageProp; }
            set
            {
                if (selectedBackgroundImageProp != value)
                {
                    selectedBackgroundImageProp = value;
                    RaisePropertyChanged(nameof(SelectedBackgroundImageProp));
                }
            }
        }

        private Variant[] availableBackgroundImageProps;
        public Variant[] AvailableBackgroundImageProps
        {
            get { return availableBackgroundImageProps; }
            set
            {
                if (availableBackgroundImageProps != value)
                {
                    availableBackgroundImageProps = value;
                    RaisePropertyChanged(nameof(AvailableBackgroundImageProps));
                }
            }
        }

        public VisualColumnDefinitionVM(ILayersTemplateSource layersTemplateSource)
        {
            this.layersTemplateSource = layersTemplateSource;
            Initialize();
        }

        private void Initialize()
        {
            List<Variant> textVariants = new List<Variant>();

            foreach (Property p in layersTemplateSource.Template)
            {
                bool foundBackgroundImage = false;                
                foreach (Class c in p.Classes)
                {
                    if (!foundBackgroundImage && !string.IsNullOrEmpty(c.BackgroundPatternSVG))
                    {
                        textVariants.Add(new Variant(p.ID, p.Name, Presentation.BackgroundImage));
                        foundBackgroundImage = true;
                    }                    
                }
            }

            AvailableBackgroundImageProps = textVariants.ToArray();
        }

        #region Serialization
        protected VisualColumnDefinitionVM(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource", typeof(ILayersTemplateSource));
            Initialize();
            string selectePropID = info.GetString("SelectedPropID");
            if (selectePropID != null)
            {
                //setting the user choice but only if it is avaialabe in loaded template
                Presentation presentationToSet = (Presentation)info.GetValue("SelectedPresentation", typeof(Presentation));

                selectedBackgroundImageProp = AvailableBackgroundImageProps.FirstOrDefault(v => (v.PropID == selectePropID) && (v.Presentation == presentationToSet));
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LayersTemplateSource", layersTemplateSource);
            info.AddValue("SelectedPropID", (selectedBackgroundImageProp == null) ? (null) : (selectedBackgroundImageProp.PropID));
            info.AddValue("SelectedPresentation", (selectedBackgroundImageProp == null) ? (Presentation.Acronym) : (selectedBackgroundImageProp.Presentation));
        }
        #endregion
    }
}
