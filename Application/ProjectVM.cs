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

        private Property[] activeLayerTemplate;

        public Property[] ActiveLayerTemplate
        {
            get { return activeLayerTemplate; }
            set
            {
                if (activeLayerTemplate != value)
                {
                    activeLayerTemplate = value;
                    RaisePropertyChanged(nameof(ActiveLayerTemplate));
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

            activeLayerTemplate = Persistence.HardcodedLayerTemplate.Instance.Template;

            planeColumnSettingsVM = new ColumnSettingsVM(ActiveLayerTemplate);
            planeColumnSettingsVM.AddDepthCommand.Execute(null);
            planeColumnSettingsVM.AddPhotoCommand.Execute(null);
        }

        public ProjectVM(IImageStorage imageStorage) {            
            boreIntervalsVM = new BoreIntervalsVM(imageStorage);
            Initialize();
        }

        #region Serialization

        protected ProjectVM(SerializationInfo info, StreamingContext context) {
            boreName = info.GetString("BoreName");
            boreIntervalsVM = (BoreIntervalsVM)info.GetValue("Intervals", typeof(BoreIntervalsVM));
            PlaneVM = (AnnotationPlane.PlaneVM)info.GetValue("Annotation", typeof(AnnotationPlane.PlaneVM));                        
            Initialize();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("BoreName", BoreName);
            info.AddValue("Intervals", BoreIntervalsVM);
            info.AddValue("Annotation",PlaneVM);
        }
        #endregion
    }
}
