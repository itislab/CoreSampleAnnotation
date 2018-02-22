using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{

    public class FilteringBoundaryCollection : ViewModel, ILayerBoundariesVM
    {
        private LayerBoundaryEditorVM target;

        private Func<LayerBoundary, bool> isBoundaryPermited;

        public LayerBoundary[] Boundaries
        {
            get
            {
                return target.Boundaries.Where(b => isBoundaryPermited(b)).ToArray();
            }
        }

        public FilteringBoundaryCollection(LayerBoundaryEditorVM target, Func<LayerBoundary,bool> isBoundaryPermited) {
            this.isBoundaryPermited = isBoundaryPermited;
            this.target = target;
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LayerBoundaryEditorVM.Boundaries):
                    RaisePropertyChanged(nameof(Boundaries));
                    break;
            }
        }
    }

    /// <summary>
    /// Exposes LayerBoundaries collection filtered to specific Rank value
    /// </summary>
    public class RankMatchingBoundaryCollection : FilteringBoundaryCollection, ILayerBoundariesVM
    {
        private readonly int rank;
        
        public int Rank {
            get { return rank; }
        }
                
        public RankMatchingBoundaryCollection(LayerBoundaryEditorVM target, int rank):
            base(target, lb => lb.Rank == rank)
        {
            this.rank = rank;   
        }        
    }

    /// <summary>
    /// Exposes LayerBoundaries collection filtered to specific Rank value
    /// </summary>
    public class RankMoreOrEqualBoundaryCollection : FilteringBoundaryCollection, ILayerBoundariesVM
    {
        private readonly int rank;

        public int Rank
        {
            get { return rank; }
        }

        public RankMoreOrEqualBoundaryCollection(LayerBoundaryEditorVM target, int rank):
            base(target, lb => lb.Rank >= rank)
        {
            this.rank = rank;
        }
    }
}
