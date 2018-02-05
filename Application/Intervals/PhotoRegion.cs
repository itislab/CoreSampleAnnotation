using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSampleAnnotation.Intervals
{
    public class PhotoRegion
    {
        public Stream BitmapStream { get; private set; }
        public Size ImageSize { get; private set; }

        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double UpperBound { get; private set; }
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double LowerBound { get; private set; }

        public PhotoRegion(Stream stream, Size size, double lower, double upper) {
            this.BitmapStream = stream;
            this.ImageSize = size;
            UpperBound = upper;
            LowerBound = lower;
        }
    }
}
