using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    [Serializable]
    public class LayerPropertyValue {
        public string Remarks { get; set; }
        public string[] Value { get; set; }
    }

    [Serializable]
    public class ColumnValues {
        public string PropID { get; set; }
        public LayerPropertyValue[] LayerValues {get; set;}
    }

    [Serializable]
    public class LayersAnnotation
    {
        /// <summary>
        /// In meters (positive values)
        /// </summary>
        public double[] LayerBoundaries { get; set;  }

        public ColumnValues[] Columns;        
    }
}
