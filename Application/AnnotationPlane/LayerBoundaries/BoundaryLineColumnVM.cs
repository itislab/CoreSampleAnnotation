using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    public class BoundaryLineColumnVM: BoundaryColumnVM
    {
        public BoundaryLineColumnVM(ColumnVM targetColumn, ILayerBoundariesVM boundariesVM, Color linesColor) :base(targetColumn, boundariesVM) {
            this.linesColor = linesColor;            
        }
        
        private Color linesColor;
        public Color LinesColor {
            get { return linesColor; }
            set {
                if (linesColor != value) {
                    linesColor = value;
                    RaisePropertyChanged(nameof(LinesColor));
                    RaisePropertyChanged(nameof(LineStrokeBrush));
                }
            }
        }

        public Brush LineStrokeBrush {
            get {
                return new SolidColorBrush(LinesColor);
            }
        }        
    }
}
