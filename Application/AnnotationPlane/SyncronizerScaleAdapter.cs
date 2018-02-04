using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class SyncronizerScaleAdapter: ColumnScale.IColumn
    {
        private LayerSyncronization.Controller syncronizer = null;
        public SyncronizerScaleAdapter(LayerSyncronization.Controller target) {
            syncronizer = target;
        }

        public double ColumnHeight
        {
            set
            {
                var newScaleFactor = value / (syncronizer.LowerDepth - syncronizer.UpperDepth);
                syncronizer.ScaleFactor = newScaleFactor;
            }
        }

        public double LowerDepth
        {
            set
            {
                syncronizer.LowerDepth = value;
            }
        }

        public double UpperDepth
        {
            set
            {
                syncronizer.UpperDepth = value;
            }
        }
    }
}
