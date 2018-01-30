using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.PhotoMarkup
{
    class ExtractionInfoVM : ViewModel
    {
        private string extractionName = string.Empty;
        public string ExtractionName {
            get {
                return extractionName;
            }

            set {
                if (extractionName != value) {
                    extractionName = value;
                    RaisePropertyChanged(nameof(ExtractionName));
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
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
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
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                }
            }
        }

        private double annotatedLength = 0;
        /// <summary>
        /// A total length in meters of all annotated parts of the core sample
        /// </summary>
        public double AnnotatedLength {
            get { return annotatedLength; }
            set {
                if (annotatedLength != value) {
                    annotatedLength = value;
                    RaisePropertyChanged(nameof(AnnotatedLength));                    
                    RaisePropertyChanged(nameof(AnnotatedPercentage));
                }
            }
        }

        public double ExtractionLength {
            get {
                return (LowerDepth - UpperDepth);
            }
        }

        public double AnnotatedPercentage {
            get {
                return AnnotatedLength / ExtractionLength * 100.0;
            }
        }
    }
}
