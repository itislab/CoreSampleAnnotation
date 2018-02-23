using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation
{
    public interface ILayerRankNamesSource
    {
        string[] NominativeNames { get; }
        string[] GeneritiveNames { get; }
        string[] InstrumentalMultipleNames { get; }
    }
}
