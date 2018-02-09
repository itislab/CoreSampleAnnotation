using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.Intervals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        private AnnotationPlane.PlaneVM planeVM = new AnnotationPlane.PlaneVM();

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

        private ColumnSettingsVM planeColumnSettingsVM = new ColumnSettingsVM();

        public ColumnSettingsVM PlaneColumnSettingsVM {
            get { return planeColumnSettingsVM; }
            set {
                if (planeColumnSettingsVM != value) {
                    planeColumnSettingsVM = value;
                    RaisePropertyChanged(nameof(PlaneColumnSettingsVM));
                }
            }
        }

        public ProjectVM(IImageStorage imageStorage) {            
            boreIntervalsVM = new BoreIntervalsVM(imageStorage);
        }

        #region Serialization

        protected ProjectVM(SerializationInfo info, StreamingContext context) {
            boreName = info.GetString("BoreName");
            boreIntervalsVM = (BoreIntervalsVM)info.GetValue("Intervals", typeof(BoreIntervalsVM));                        
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("BoreName", BoreName);
            info.AddValue("Intervals", BoreIntervalsVM);
        }
        #endregion
    }
}
