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

        private double upperDepth = double.NaN;
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
                    RaisePropertyChanged(nameof(MaxPossibleExtractionLength));                    
                }
            }
        }

        private double lowerDepth = double.NaN;
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
                    RaisePropertyChanged(nameof(MaxPossibleExtractionLength));
                }
            }
        }
        
        public double MaxPossibleExtractionLength {
            get {
                return (LowerDepth - UpperDepth);
            }
        }        
    }
}
