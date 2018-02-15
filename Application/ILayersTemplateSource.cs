using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation
{
    public interface ILayersTemplateSource
    {
        AnnotationPlane.Template.Property[] Template { get; }
    }
}
