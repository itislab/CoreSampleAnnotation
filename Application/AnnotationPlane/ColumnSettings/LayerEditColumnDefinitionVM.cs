using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    [Serializable]
    public class LayerEditColumnDefinitionVM : ColumnDefinitionVM, ISerializable
    {
        public string[] RankNames { get; private set; }
        public ILayerRankNamesSource NameSource { get; private set; }

        private string selectedRankName;
        public string Selected {
            get { return selectedRankName; }
            set {
                if (selectedRankName != value) {
                    selectedRankName = value;
                    RaisePropertyChanged(nameof(Selected));
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        public int SelectedIndex {
            get {
                return Array.IndexOf(RankNames, Selected);
            }
        }

        public LayerEditColumnDefinitionVM(ILayerRankNamesSource source) {
            this.NameSource = source;
            this.RankNames = source.InstrumentalMultipleNames;
        }

        #region Serialization

        protected LayerEditColumnDefinitionVM(SerializationInfo info, StreamingContext context):base(info,context)
        {
            NameSource = (ILayerRankNamesSource)info.GetValue("RankNameSource",typeof(ILayerRankNamesSource));
            RankNames = NameSource.InstrumentalMultipleNames;
            string selected = info.GetString("Selected");
            if (!string.IsNullOrEmpty(selected) && RankNames.Contains(selected))
                Selected = selected;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info,context);            
            info.AddValue("Selected", Selected);
            info.AddValue("RankNameSource", NameSource);
        }
        #endregion

    }
}
