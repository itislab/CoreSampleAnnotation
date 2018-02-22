using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class SyncronizerBoundaryAdapter : ColumnScale.IColumn
    {
        private double columnBottum;
        private LayerBoundaryEditorVM boundariesVM;

        public double ColumnHeight
        {
            set
            {
                columnBottum = value;
            }
        }

        public double UpperDepth
        {
            set
            { }
        }

        public double LowerDepth
        {
            set
            { }
        }

        public SyncronizerBoundaryAdapter(LayerBoundaryEditorVM lbeVM)
        {
            boundariesVM = lbeVM;
        }      
    }
}
