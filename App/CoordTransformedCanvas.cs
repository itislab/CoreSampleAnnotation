using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace All
{
    /// <summary>
    /// Propagetes "CoordsTransform" property value to Canvas childen (IInfoLayerElement only)
    /// </summary>
    public class CoordTransformedCanvas : Canvas
    {
        public Transform CoordsTransform
        {
            get { return (Transform)GetValue(CoordsTransformProperty); }
            set { SetValue(CoordsTransformProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CoordsTransform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoordsTransformProperty =
            DependencyProperty.Register("CoordsTransform", typeof(Transform), typeof(CoordTransformedCanvas), new PropertyMetadata(null, (obj, args) => {
                if ((args.OldValue != null) && (args.NewValue != null))
                {
                    var canvas = obj as CoordTransformedCanvas;

                    var orig = args.OldValue as Transform;
                    var newVal = args.NewValue as Transform;

                    var origInv = (Transform)orig.Inverse;

                    foreach (var child in canvas.Children) {
                        IInfoLayerElement ile = child as IInfoLayerElement;
                        if (ile != null) {
                            ile.ChangeCoordTransform(origInv, newVal);
                        }
                    }
                }
            }));
    }
}
