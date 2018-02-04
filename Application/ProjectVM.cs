using CoreSampleAnnotation.Intervals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation
{
    public class ProjectVM: ViewModel
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

        private BoreIntervalsVM boreIntervalsVM = new BoreIntervalsVM();
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
    }
}
