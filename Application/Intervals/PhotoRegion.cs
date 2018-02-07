using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CoreSampleAnnotation.Intervals
{    
    public class PhotoRegion: ViewModel
    {
        public BitmapImage BitmapImage { get; private set; }
        public Size ImageSize { get; private set; }

        private double wpfWidth = 0.0;
        /// <summary>
        /// How match is actual width of the image in the column
        /// </summary>
        public double WpfWidth {
            get {
                return wpfWidth;
            }
            set {
                if (wpfWidth != value) {
                    wpfWidth = value;
                    RaisePropertyChanged(nameof(WpfWidth));
                }
            }
        }

        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double ImageUpperDepth { get; private set; }
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double ImageLowerDepth { get; private set; }

        public PhotoRegion(BitmapImage bitMapImage, Size size, double upper,double lower) {
            this.BitmapImage = bitMapImage;
            this.ImageSize = size;
            ImageUpperDepth = upper;
            ImageLowerDepth = lower;
        }

        
    }
}
