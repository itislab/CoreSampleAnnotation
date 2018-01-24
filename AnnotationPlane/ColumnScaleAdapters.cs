using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationPlane
{
    public class ColVMAdapter : ColumnScale.IColumn
    {
        private ColumnVM vm = null;

        public ColVMAdapter(ColumnVM target) {
            vm = target;
        }

        public double ColumnHeight
        {
            set
            {
                vm.ColumnHeight = value;
            }
        }

        public double LowerDepth
        {
            set
            {
                vm.LowerBound = value;
            }
        }

        public double UpperDepth
        {
            set
            {
                vm.UpperBound = value;
            }
        }
    }
}
