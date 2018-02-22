using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.AnnotationPlane.Template;
using CoreSampleAnnotation.Intervals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation
{    
    [Serializable]
    public class ProjectVM : ViewModel, ISerializable
    {
        private string boreName = string.Empty;

        private ILayerRankNamesSource layerRankNameSource;
        public ILayerRankNamesSource LayerRankNameSource {
            get
            { return layerRankNameSource; }
            set {
                if (layerRankNameSource != value) {
                    layerRankNameSource = value;
                    RaisePropertyChanged(nameof(LayerRankNameSource));
                }
            }
        }

        /// <summary>
        /// The name og the bore hole from where the core samples are extracted
        /// </summary>
        public string BoreName {
            get {
                return boreName;
            }
            set {
                if (boreName != value) {
                    boreName = value;
                    RaisePropertyChanged(nameof(BoreName));
                }
            }
        }

        private ILayersTemplateSource layersTemplateSource;

        public ILayersTemplateSource LayersTemplateSource
        {
            get { return layersTemplateSource; }
            set
            {
                if (layersTemplateSource != value)
                {
                    layersTemplateSource = value;
                    RaisePropertyChanged(nameof(LayersTemplateSource));
                }
            }
        }


        private BoreIntervalsVM boreIntervalsVM;
        public BoreIntervalsVM BoreIntervalsVM {
            get { return boreIntervalsVM; }
            set {
                if (boreIntervalsVM != value) {
                    boreIntervalsVM = value;
                    RaisePropertyChanged(nameof(BoreIntervalsVM));
                }
            }
        }

        private AnnotationPlane.PlaneVM planeVM = null;

        public AnnotationPlane.PlaneVM PlaneVM {
            get {
                return planeVM;
            }
            set {
                if (value != planeVM) {
                    planeVM = value;
                    RaisePropertyChanged(nameof(PlaneVM));
                }
            }
        }

        private ColumnSettingsVM planeColumnSettingsVM;

        public ColumnSettingsVM PlaneColumnSettingsVM {
            get { return planeColumnSettingsVM; }
            set {
                if (planeColumnSettingsVM != value) {
                    planeColumnSettingsVM = value;
                    RaisePropertyChanged(nameof(PlaneColumnSettingsVM));
                }
            }
        }

        public void Initialize() {
            layerRankNameSource = new Persistence.StaticLayerRankNamesSource();
            if(planeColumnSettingsVM == null)
                planeColumnSettingsVM = new ColumnSettingsVM(LayersTemplateSource,LayerRankNameSource);            
        }

        public ProjectVM(IImageStorage imageStorage,ILayersTemplateSource layersTamplateSource) {            
            boreIntervalsVM = new BoreIntervalsVM(imageStorage);
            this.layersTemplateSource = layersTamplateSource;            
            Initialize();
        }

        #region Serialization

        protected ProjectVM(SerializationInfo info, StreamingContext context) {
            boreName = info.GetString("BoreName");
            boreIntervalsVM = (BoreIntervalsVM)info.GetValue("Intervals", typeof(BoreIntervalsVM));
            PlaneVM = (AnnotationPlane.PlaneVM)info.GetValue("Annotation", typeof(AnnotationPlane.PlaneVM));
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource",typeof(ILayersTemplateSource));
            planeColumnSettingsVM = (ColumnSettingsVM)info.GetValue("ColumnsSettings", typeof(ColumnSettingsVM));
            Initialize();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("BoreName", BoreName);
            info.AddValue("Intervals", BoreIntervalsVM);
            info.AddValue("Annotation",PlaneVM);
            info.AddValue("LayersTemplateSource", LayersTemplateSource);
            info.AddValue("ColumnsSettings", PlaneColumnSettingsVM);
        }
        #endregion
    }
}
