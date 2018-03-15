using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    /// <summary>
    /// Adds boundary at 0 level
    /// </summary>
    public class ZeroBoundaryDecoratorLBVM : ViewModel, ILayerBoundariesVM
    {
        ILayerBoundariesVM target;

        public ZeroBoundaryDecoratorLBVM(ILayerBoundariesVM target) {
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

        LayerBoundary zero = new LayerBoundary(0, 0);

        public LayerBoundary[] Boundaries
        {
            get
            {
                LayerBoundary[] boundaries = Enumerable.Repeat(zero, 1).Concat(target.Boundaries).ToArray();
                return boundaries;
            }
        }
    }


    public class NumberResettingDecorator : ViewModel, ILayerBoundariesVM
    {
        ILayerBoundariesVM target;

        int thresholdRank, numberingBase;

        /// <param name="thresholdRank">Rank higher than this resets the numbering to numbering base</param>
        public NumberResettingDecorator(ILayerBoundariesVM target, int thresholdRank, int numberingBase)
        {
            this.target = target;
            this.thresholdRank = thresholdRank;
            this.numberingBase = numberingBase;
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
                LayerBoundary[] boundaries = target.Boundaries.Select( lb => new LayerBoundary(lb.Level,lb.Rank)).ToArray(); //deep copy
                int n = boundaries.Length;
                int curNumber = numberingBase;
                for (int i = 0; i < n; i++)
                {
                    LayerBoundary lb = boundaries[i];
                    if (lb.Rank > thresholdRank)
                        //reset
                        curNumber = numberingBase;                    
                    lb.Number = curNumber++;
                }
                return boundaries;
            }
        }
    }

    /// <summary>
    /// Used to display text labels of boundaries
    /// </summary>
    public class BoundaryLabelColumnVM: BoundaryColumnVM
    {
        public BoundaryLabelColumnVM(ColumnVM targetColumn, ILayerBoundariesVM boundariesVM)
            : base(targetColumn, boundariesVM) { }
    }
}
