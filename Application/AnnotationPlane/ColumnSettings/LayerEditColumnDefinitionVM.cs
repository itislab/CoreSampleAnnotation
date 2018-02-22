using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    public class LayerEditColumnDefinitionVM : ColumnDefinitionVM
    {
        public string[] RankNames { get; private set; }

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

        public LayerEditColumnDefinitionVM(string[] rankNames) {
            this.RankNames = rankNames;
        }
    
    }
}
