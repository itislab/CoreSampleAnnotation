using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane.Template
{
    /// <summary>
    /// A class that one property can take
    /// </summary>
    public class Class
    {
        public string ID { get; set; }
        public Color? Color { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }

        public string Acronym { get; set; }

        public InputField[] InputFields { get; set; }
        
    }
}
