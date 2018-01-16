using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace All
{
    public class CalibratedRegion : ViewModel , IDisposable
    {
        private PhotoCalibrationMarker up;
        private PhotoCalibrationMarker bottom;
        private PhotoCalibrationMarker side;
        private AnnotatedPolygon poly;

        private bool isFocused = false;
        public bool IsFocused {
            get {
                return this.isFocused;
            }
            set {
                if (value != isFocused) {
                    this.isFocused = value;
                    this.RaisePropertyChanged(nameof(IsFocused));
                }
            }
        }

        public CalibratedRegion(PhotoCalibrationMarker up, PhotoCalibrationMarker bottom, PhotoCalibrationMarker side, AnnotatedPolygon poly)
        {
            this.up = up;
            this.bottom = bottom;
            this.side = side;
            this.poly = poly;

            var b1 = new Binding(nameof(bottom.CentreLocation));
            b1.Source = bottom;
            poly.SetBinding(AnnotatedPolygon.BottomCentreProperty, b1);

            var b2 = new Binding(nameof(up.CentreLocation));
            b2.Source = up;
            poly.SetBinding(AnnotatedPolygon.UpCentreProperty, b2);

            var b3 = new Binding(nameof(side.CentreLocation));
            b3.Source = side;
            poly.SetBinding(AnnotatedPolygon.SideProperty, b3);
        }

        public void Dispose()
        {
            BindingOperations.ClearBinding(this.poly, AnnotatedPolygon.BottomCentreProperty);
            BindingOperations.ClearBinding(this.poly, AnnotatedPolygon.UpCentreProperty);
            BindingOperations.ClearBinding(this.poly, AnnotatedPolygon.SideProperty);
        }
    }
}
