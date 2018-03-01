using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public interface ILayerColumn
    {
        ObservableCollection<LayerVM> Layers { get; }
    }
}
