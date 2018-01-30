using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Intervals
{
    public class BoreIntervalVM : ViewModel
    {
        private double extractedLength = double.NaN;
        /// <summary>
        /// The length (im meters) of extracted meterial from the bore
        /// </summary>
        public double ExtractedLength
        {
            get {
                return extractedLength;
            }

            set {
                if (extractedLength != value) {
                    extractedLength = value;
                    RaisePropertyChanged(nameof(ExtractedLength));
                }
            }
        }

        private double upperDepth = 0;
        /// <summary>
        /// A depth (positive value) in meters of the highest extraction point
        /// </summary>
        public double UpperDepth
        {
            get
            {
                return upperDepth;
            }

            set
            {
                if (upperDepth != value)
                {
                    upperDepth = value;
                    RaisePropertyChanged(nameof(UpperDepth));
                    RaisePropertyChanged(nameof(ExtractionLength));                    
                }
            }
        }

        private double lowerDepth = 0;
        /// <summary>
        /// A depth (positive value) in meters of the loweret extraction point
        /// </summary>
        public double LowerDepth
        {
            get
            {
                return lowerDepth;
            }

            set
            {
                if (lowerDepth != value)
                {
                    lowerDepth = value;
                    RaisePropertyChanged(nameof(LowerDepth));
                    RaisePropertyChanged(nameof(ExtractionLength));
                }
            }
        }
        
        public double ExtractionLength {
            get {
                return (LowerDepth - UpperDepth);
            }
        }        
    }
}
