using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{    
    public class LayersAnnotation
    {
        public double[] LayerBoundaries { get; set;  }
        public Dictionary<string, string[]>[] LayerAnnotation { get; set; }
    }
}
