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
    public class IconsColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        private ILayersTemplateSource layersTemplateSource;

        private Variant selectedIconProp;
        public Variant SelectedIconProp
        {
            get { return selectedIconProp; }
            set
            {
                if (selectedIconProp != value)
                {
                    selectedIconProp = value;
                    RaisePropertyChanged(nameof(SelectedIconProp));
                }
            }
        }

        private Variant[] availableIconProps;
        public Variant[] AvailableIconProps
        {
            get { return availableIconProps; }
            set
            {
                if (availableIconProps != value)
                {
                    availableIconProps = value;
                    RaisePropertyChanged(nameof(AvailableIconProps));
                }
            }
        }

        public IconsColumnDefinitionVM(ILayersTemplateSource layersTemplateSource)
        {
            this.layersTemplateSource = layersTemplateSource;
            Initialize();
        }

        private void Initialize()
        {
            List<Variant> iconVariants = new List<Variant>();

            foreach (Property p in layersTemplateSource.Template)
            {
                bool foundIconImage = false;
                foreach (Class c in p.Classes)
                {
                    if (!foundIconImage && !string.IsNullOrEmpty(c.IconSVG))
                    {
                        iconVariants.Add(new Variant(p.ID, p.Name, Presentation.Icon));
                        foundIconImage = true;
                    }
                }
            }

            AvailableIconProps = iconVariants.ToArray();
        }

        #region Serialization
        protected IconsColumnDefinitionVM(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource", typeof(ILayersTemplateSource));
            Initialize();
            string selectePropID = info.GetString("SelectedPropID");
            if (selectePropID != null)
            {
                //setting the user choice but only if it is avaialabe in loaded template
                Presentation presentationToSet = (Presentation)info.GetValue("SelectedPresentation", typeof(Presentation));

                selectedIconProp = AvailableIconProps.FirstOrDefault(v => (v.PropID == selectePropID) && (v.Presentation == presentationToSet));
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LayersTemplateSource", layersTemplateSource);
            info.AddValue("SelectedPropID", (selectedIconProp == null) ? (null) : (selectedIconProp.PropID));
            info.AddValue("SelectedPresentation", (selectedIconProp == null) ? (Presentation.Acronym) : (selectedIconProp.Presentation));
        }
        #endregion
    }
}
