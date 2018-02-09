using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.Template
{
    public abstract class InputField
    {
        string Name { get; set; }
    }

    public class TextInputField : InputField {
    }

    public class PropertyInputField : InputField {
        public Property Property { get; set; }
    }


}
