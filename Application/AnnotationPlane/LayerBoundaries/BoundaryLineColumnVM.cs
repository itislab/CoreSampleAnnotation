using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    public class BoundaryLineColumnVM: ColumnVM
    {

        public BoundaryLineColumnVM(ColumnVM targetColumn, ILayerBoundariesVM boundariesVM, Color linesColor) :base(targetColumn.Heading) {
            ColumnVM = targetColumn;
            BoundariesVM = boundariesVM;
            this.linesColor = linesColor;
            PropertyChanged += BoundaryLineColumnVM_PropertyChanged;
        }

        private void BoundaryLineColumnVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(ColumnHeight):
                    ColumnVM.ColumnHeight = ColumnHeight;
                    break;
                case nameof(UpperBound):
                    ColumnVM.UpperBound = UpperBound;
                    break;
                case nameof(LowerBound):
                    ColumnVM.LowerBound = LowerBound;
                    break;
            }
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

        public ILayerBoundariesVM BoundariesVM { get; private set; }
        public ColumnVM ColumnVM { get; private set; }
    }
}
