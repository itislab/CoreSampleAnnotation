using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.Template
{
    /// <summary>
    /// A property of the layer
    /// </summary>
    public class Property
    {
        /// <summary>
        /// All available classes that this property can take
        /// </summary>
        public Class[] Classes { get; set; }
        /// <summary>
        /// Is it possible to simulteniously contain several classes
        /// </summary>
        public bool IsMulticlass { get; set; }

        public string Name { get; set; }

        public string ID { get; set; }
    }    
}
