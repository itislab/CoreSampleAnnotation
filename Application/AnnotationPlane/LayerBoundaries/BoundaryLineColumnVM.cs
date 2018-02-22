using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    public class BoundaryLineColumnVM: ColumnVM
    {

        public BoundaryLineColumnVM(ColumnVM targetColumn, ILayerBoundariesVM boundariesVM) :base(targetColumn.Heading) {
            ColumnVM = targetColumn;
            BoundariesVM = boundariesVM;
        }


        public ILayerBoundariesVM BoundariesVM { get; private set; }
        public ColumnVM ColumnVM { get; private set; }
    }
}
