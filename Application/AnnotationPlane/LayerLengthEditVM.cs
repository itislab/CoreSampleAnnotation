using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class LayerLengthEditVM : ViewModel
    {
        private readonly int layerIdx;

        public int LayerIdx {
            get { return layerIdx; }
        }

        private double layerLength;
        /// <summary>
        /// In meteres (positive value)
        /// </summary>
        public double LayerLength {
            get { return layerLength; }
            set {
                if (layerLength != value) {
                    layerLength = value;
                    RaisePropertyChanged(nameof(LayerLength));
                }
            }
        }

        public LayerLengthEditVM(int layerIdx) {
            this.layerIdx = layerIdx;
        }
    }
}
