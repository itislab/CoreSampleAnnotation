using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    /// <summary>
    /// Removes the last boundary in the array
    /// </summary>
    public class RemoveLastDecoratorLBVM : ViewModel, ILayerBoundariesVM
    {
        ILayerBoundariesVM target;

        public RemoveLastDecoratorLBVM(ILayerBoundariesVM target) {
            this.target = target;
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(ILayerBoundariesVM.Boundaries):
                    RaisePropertyChanged(nameof(Boundaries));
                    break;
            }
        }        

        public LayerBoundary[] Boundaries
        {
            get
            {
                LayerBoundary[] boundaries = target.Boundaries.Take(target.Boundaries.Length - 1).ToArray();
                return boundaries;
            }
        }
    }

    /// <summary>
    /// Removes the first boundary in the array
    /// </summary>
    public class RemoveFirstDecoratorLBVM : ViewModel, ILayerBoundariesVM
    {
        ILayerBoundariesVM target;

        public RemoveFirstDecoratorLBVM(ILayerBoundariesVM target)
        {
            this.target = target;
            target.PropertyChanged += Target_PropertyChanged;
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ILayerBoundariesVM.Boundaries):
                    RaisePropertyChanged(nameof(Boundaries));
                    break;
            }
        }

        public LayerBoundary[] Boundaries
        {
            get
            {
                LayerBoundary[] boundaries = target.Boundaries.Skip(1).ToArray();
                return boundaries;
            }
        }
    }

    /// <summary>
    /// Used to display text labels of boundaries
    /// </summary>
    public class BoundaryLabelColumnVM: BoundaryColumnVM
    {
        /// <summary>
        /// Number of which rank will be shown on labels
        /// </summary>
        public int RankNumberToShow { get; private set; }

        /// <param name="rankNumberToShow">Number of which rank will be shown on labels</param>
        public BoundaryLabelColumnVM(ColumnVM targetColumn, ILayerBoundariesVM boundariesVM, int rankNumberToShow)
            : base(targetColumn, boundariesVM) {
            RankNumberToShow = rankNumberToShow;
        }
    }
}
