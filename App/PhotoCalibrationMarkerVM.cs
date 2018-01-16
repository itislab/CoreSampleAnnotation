using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace All
{
    class PhotoCalibrationMarkerVM : ViewModel
    {
        private string markerName = string.Empty;

        public string MarkerName
        {
            get
            {
                return markerName;
            }
            set
            {
                if (this.markerName != value)
                {
                    this.markerName = value;
                    RaisePropertyChanged(nameof(MarkerName));
                }
            }
        }

        private Brush fillBrush = null;
        public Brush FillBrush
        {
            get
            {
                return fillBrush;
            }
            set {
                if (value != fillBrush) {
                    fillBrush = value;
                    RaisePropertyChanged(nameof(FillBrush));
                }
            }
        }
    }
}
