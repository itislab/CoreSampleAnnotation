using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class LayerPropertyValue {
        public string Remarks { get; set; }
        public string Value { get; set; }
    }

    public class ColumnValues {
        public string PropID { get; set; }
        public LayerPropertyValue[] LayerValues {get; set;}
    }

    public class LayersAnnotation
    {
        public double[] LayerBoundaries { get; set;  }

        public ColumnValues[] Columns;
    }
}
