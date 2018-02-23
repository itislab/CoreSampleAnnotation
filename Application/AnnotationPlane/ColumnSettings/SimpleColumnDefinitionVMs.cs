using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    [Serializable]
    public class DepthColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        public DepthColumnDefinitionVM() { }

        protected DepthColumnDefinitionVM(SerializationInfo info, StreamingContext context):base(info,context) { }
    }

    [Serializable]
    public class LayerSamplesDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        public LayerSamplesDefinitionVM() { }

        protected LayerSamplesDefinitionVM(SerializationInfo info, StreamingContext context):base(info,context) { }
    }

    [Serializable]
    public class LayerLengthColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        public LayerLengthColumnDefinitionVM() { }

        protected LayerLengthColumnDefinitionVM(SerializationInfo info, StreamingContext context):base(info,context) { }
    }

    [Serializable]
    public class PhotoColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        public PhotoColumnDefinitionVM() { }

        protected PhotoColumnDefinitionVM(SerializationInfo info, StreamingContext context):base(info,context) { }
    }
}
