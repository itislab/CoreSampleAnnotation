using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class BlankColumnVM : ColumnVM
    {
        private double columnWidth;

        /// <summary>
        /// In WPF units
        /// </summary>
        public double ColumnWidth {
            get {
                return columnWidth;
            }
            set {
                if (columnWidth != value) {
                    columnWidth = value;
                    RaisePropertyChanged(nameof(ColumnWidth));
                }
            }
        }

        public BlankColumnVM(string heading) : base(heading)
        {
        }
    }
}
