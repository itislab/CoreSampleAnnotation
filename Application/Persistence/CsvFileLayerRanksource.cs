using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.Persistence
{
    [DelimitedRecord(",")]
    public class RankFileRow
    {
        public string Nominative;
        public string Generitive;
        public string InstrumentalMultiple;        
    }

    [Serializable]
    public class CsvFileLayerRankSource : ILayerRankNamesSource, ISerializable
    {
        private string rankFilepFullPath;
        private RankFileRow[] names;

        public CsvFileLayerRankSource(string rankFilepFullPath) {
            this.rankFilepFullPath = rankFilepFullPath;
        }

        private void LoadArrays()
        {
            if (File.Exists(rankFilepFullPath))
            {
                var engine = new FileHelperEngine<RankFileRow>();
                var rows = engine.ReadFile(rankFilepFullPath);
                List<RankFileRow> loaded = new List<RankFileRow>();
                foreach (RankFileRow row in rows)
                    loaded.Add(row);
                names = loaded.ToArray();
            }
            else throw new InvalidDataException("Не найден файл с именами групп слоев");
        }


        public string[] GeneritiveNames
        {
            get
            {
                if (names == null)
                    LoadArrays();
                return names.Select(n => n.Generitive).ToArray();
            }
        }

        public string[] InstrumentalMultipleNames
        {
            get
            {
                if (names == null)
                    LoadArrays();
                return names.Select(n => n.InstrumentalMultiple).ToArray();
            }
        }

        public string[] NominativeNames
        {
            get
            {
                if (names == null)
                    LoadArrays();
                return names.Select(n => n.Nominative).ToArray();
            }
        }

        #region serialization
        protected CsvFileLayerRankSource(SerializationInfo info, StreamingContext context) {
            rankFilepFullPath = Path.Combine(Directory.GetCurrentDirectory(), info.GetString("File"));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string relativePath = PathUtils.MakeRelativePath(Directory.GetCurrentDirectory() + "\\", rankFilepFullPath);
            info.AddValue("File", relativePath);
        }

        #endregion
    }
}
