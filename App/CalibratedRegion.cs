using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace All
{
    public class CalibratedRegion
    {
        public CalibratedRegion(PhotoCalibrationMarker up, PhotoCalibrationMarker bottom, PhotoCalibrationMarker side, AnnotatedPolygon poly)
        {
            var b1 = new Binding(nameof(bottom.CentreLocation));
            b1.Source = bottom;
            poly.SetBinding(AnnotatedPolygon.BottomCentreProperty, b1);

            var b2 = new Binding(nameof(up.CentreLocation));
            b2.Source = up;
            poly.SetBinding(AnnotatedPolygon.UpCentreProperty, b2);

            var b3 = new Binding(nameof(side.CentreLocation));
            b3.Source = side;
            poly.SetBinding(AnnotatedPolygon.SideProperty,b3);
        }
    }
}
