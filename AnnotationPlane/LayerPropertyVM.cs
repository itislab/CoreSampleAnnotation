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

        public virtual LayerVM DeepClone() {
            var result = new LayerVM();
            result.Length = Length;
            return result;
        }
    }

    public class LengthLayerVM : LayerVM {
        private double realLength = 0.0;

        /// <summary>
        /// In meters
        /// </summary>
        public double RealLength {
            get {
                return realLength;
            }
            set {
                if (realLength != value) {
                    realLength = value;
                    RaisePropertyChanged(nameof(RealLength));
                }
            }
        }

        public override LayerVM DeepClone() {
            var result = new LengthLayerVM();
            result.Length = Length;
            result.RealLength = RealLength;
            return result;

        }
    }

    public class IconLayerVM : LayerVM {
        public override LayerVM DeepClone()
        {
            var result = new IconLayerVM();
            result.Length = Length;            
            return result;
        }
    }

    public class TextLayerVM : LayerVM {
        private string text = string.Empty;

        public string Text {
            get { return text; }
            set {
                if (text != value) {
                    text = value;
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }

        public override LayerVM DeepClone()
        {
            var result = new TextLayerVM();
            result.Length = Length;
            result.Text = Text;
            return result;

        }
    }
}
