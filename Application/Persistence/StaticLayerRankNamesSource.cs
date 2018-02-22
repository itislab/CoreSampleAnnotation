using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Persistence
{
    public class StaticLayerRankNamesSource : ILayerRankNamesSource
    {
        public string[] GetGeneritiveNames
        {
            get
            {
                return new string[] { "Слоя", "Пачки", "Группы пачек" };
            }
        }

        public string[] GetInstrumentalMultipleNames
        {
            get
            {
                return new string[] { "Слоями", "Пачками", "Группами пачек" };
            }
        }

        public string[] GetNominativeNames
        {
            get
            {
                return new string[] { "Слой", "Пачка", "Группа пачек" };
            }
        }
    }
}
