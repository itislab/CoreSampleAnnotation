using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    /// <summary>
    /// Exposes LayerBoundaries collection filtered to specific Rank value
    /// </summary>
    public class RankFilteringBoundaryCollection : ViewModel, ILayerBoundariesVM
    {
        private readonly int rank;
        private LayerBoundaryEditorVM target;

        public int Rank {
            get { return rank; }
        }

        public LayerBoundary[] Boundaries {
            get {
                return target.Boundaries.Where(b => b.Rank == rank).ToArray();
            }
        }

        public RankFilteringBoundaryCollection(LayerBoundaryEditorVM target, int rank) {
            this.target = target;
            this.rank = rank;
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(LayerBoundaryEditorVM.Boundaries):
                    RaisePropertyChanged(nameof(Boundaries));
                    break;
            }
        }
    }
}
