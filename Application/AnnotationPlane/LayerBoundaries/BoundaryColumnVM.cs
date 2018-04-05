using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    public class BoundaryColumnVM : ColumnVM
    {
        public BoundaryColumnVM(ColumnVM targetColumn, ILayerBoundariesVM boundariesVM) : base(targetColumn.Heading)
        {
            ColumnVM = targetColumn;
            BoundariesVM = boundariesVM;
            PropertyChanged += BoundaryLineColumnVM_PropertyChanged;
        }

        private void BoundaryLineColumnVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ColumnHeight):
                    ColumnVM.ColumnHeight = ColumnHeight;
                    RaisePropertyChanged(nameof(BoundariesVM)); //to trigger recalulation of boundaries visual representation
                    break;
                case nameof(UpperBound):
                    ColumnVM.UpperBound = UpperBound;
                    RaisePropertyChanged(nameof(BoundariesVM)); //to trigger recalulation of boundaries visual representation
                    break;
                case nameof(LowerBound):
                    ColumnVM.LowerBound = LowerBound;
                    RaisePropertyChanged(nameof(BoundariesVM)); //to trigger recalulation of boundaries visual representation
                    break;
            }
        }

        public virtual ILayerBoundariesVM BoundariesVM { get; private set; }
        public ColumnVM ColumnVM { get; private set; }
    }
}
