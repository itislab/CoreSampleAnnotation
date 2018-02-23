using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Persistence
{
    [Obsolete("Used for early development")]
    public class StaticLayerRankNamesSource : ILayerRankNamesSource
    {
        public string[] GeneritiveNames
        {
            get
            {
                return new string[] { "Слоя", "Пачки", "Группы пачек" };
            }
        }

        public string[] InstrumentalMultipleNames
        {
            get
            {
                return new string[] { "Слоями", "Пачками", "Группами пачек" };
            }
        }

        public string[] NominativeNames
        {
            get
            {
                return new string[] { "Слой", "Пачка", "Группа пачек" };
            }
        }
    }
}
