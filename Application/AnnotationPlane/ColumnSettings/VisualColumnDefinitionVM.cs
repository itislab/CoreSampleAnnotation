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

        private Variant selectedWidthProp;
        public Variant SelectedWidthProp {
            get { return selectedWidthProp; }
            set {
                if (selectedWidthProp != value) {
                    selectedWidthProp = value;
                    RaisePropertyChanged(nameof(SelectedWidthProp));
                }
            }
        }

        private Variant[] availableWidthProps;
        public Variant[] AvailableWidthProps {
            get { return availableWidthProps; }
            set {
                if (availableWidthProps != value) {
                    availableWidthProps = value;
                    RaisePropertyChanged(nameof(AvailableWidthProps));
                }
            }
        }

        private Variant selectedRightSideProp;
        public Variant SelectedRightSideProp {
            get { return selectedRightSideProp; }
            set {
                if (selectedRightSideProp != value) {
                    selectedRightSideProp = value;
                    RaisePropertyChanged(nameof(SelectedRightSideProp));
                }
            }
        }

        private Variant[] availableRightSideProps;
        public Variant[] AvailableRightSideProps {
            get { return availableRightSideProps; }
            set {
                if (availableRightSideProps != value) {
                    availableRightSideProps = value;
                    RaisePropertyChanged(nameof(AvailableRightSideProps));
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
            List<Variant> backgroundVariants = new List<Variant>();
            List<Variant> widthVariants = new List<Variant>();
            List<Variant> rightSideVariants = new List<Variant>();

            foreach (Property p in layersTemplateSource.Template)
            {
                bool foundBackgroundImage = false;
                bool foundWidth = false;
                bool foundRightSide = false;
                foreach (Class c in p.Classes)
                {
                    if (!foundBackgroundImage && !string.IsNullOrEmpty(c.BackgroundPatternSVG))
                    {
                        backgroundVariants.Add(new Variant(p.ID, p.Name, Presentation.BackgroundImage));
                        foundBackgroundImage = true;
                    }
                    if (!foundWidth && !double.IsNaN(c.WidthRatio)) {
                        widthVariants.Add(new Variant(p.ID,p.Name, Presentation.Width));
                        foundWidth = true;
                    }
                    if (!foundRightSide && (c.RightSideForm != RightSideFormEnum.NotDefined)) {
                        rightSideVariants.Add(new Variant(p.ID, p.Name, Presentation.RightSide));
                        foundRightSide = true;
                    }
                }
            }
            
            AvailableBackgroundImageProps = backgroundVariants.ToArray();
            AvailableWidthProps = widthVariants.ToArray();
            AvailableRightSideProps = rightSideVariants.ToArray();
        }

        #region Serialization
        protected VisualColumnDefinitionVM(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource", typeof(ILayersTemplateSource));
            Initialize();
            string backgroundSelectedPropID = info.GetString("BackgroundSelectedPropID");
            string widthSelectedPropID = info.GetString("WidthSelectedPropID");
            string sideSelectedPropID = info.GetString("SideSelectedPropID");
            //setting the user choice but only if it is avaialabe in loaded template
            if (backgroundSelectedPropID != null)                         
                selectedBackgroundImageProp = AvailableBackgroundImageProps.FirstOrDefault(v => (v.PropID == backgroundSelectedPropID) && (v.Presentation == Presentation.BackgroundImage));
            if (widthSelectedPropID != null)
                selectedWidthProp = AvailableWidthProps.FirstOrDefault(v => (v.PropID == widthSelectedPropID) && (v.Presentation == Presentation.Width));
            if (sideSelectedPropID != null)
                selectedRightSideProp = AvailableRightSideProps.FirstOrDefault(v => (v.PropID == sideSelectedPropID) && (v.Presentation == Presentation.RightSide));            
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LayersTemplateSource", layersTemplateSource);            
            info.AddValue("BackgroundSelectedPropID", (selectedBackgroundImageProp == null) ? (null) : (selectedBackgroundImageProp.PropID));            
            info.AddValue("WidthSelectedPropID", (selectedWidthProp == null) ? (null) : (selectedWidthProp.PropID));            
            info.AddValue("SideSelectedPropID", (selectedRightSideProp == null) ? (null) : (selectedRightSideProp.PropID));            
        }
        #endregion
    }
}
