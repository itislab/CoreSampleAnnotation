using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationPlane
{    
    public class LayerVM: ViewModel
    {
        private double length = 0;
        /// <summary>
        /// Length of the layer in WPF units
        /// </summary>
        public double Length {
            get {
                return length;
            }
            set {
                if (length != value) {
                    length = value;
                    RaisePropertyChanged(nameof(Length));
                }
            }
        }
    }

    public class LengthLayerVM : LayerVM {        
    }
}
