using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationPlane.ColumnScale
{
    public interface IColumn {
        /// <summary>
        /// In WPF units
        /// </summary>
        double ColumnHeight { set; }
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        double UpperDepth { set; }
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        double LowerDepth { set; }
    }

    public class Controller : ViewModel
    {
        List<IColumn> controlledColumns = new List<IColumn>();

        private double scaleFactor = 1.0;

        /// <summary>
        /// How much 1 real meter in WPF coordinates
        /// </summary>
        public double ScaleFactor {
            get { return scaleFactor; }
            set {
                if (scaleFactor != value) {
                    scaleFactor = value;
                    RaisePropertyChanged(nameof(ScaleFactor));
                    UpdateColumns();
                }
            }
        }

        private double upperDepth = 0.0;
        
        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double UpperDepth {
            get { return upperDepth; }
            set {
                if (upperDepth != value) {
                    upperDepth = value;
                    RaisePropertyChanged(nameof(UpperDepth));
                    UpdateColumns();
                }
            }
        }

        private double lowerDepth = 1.0;

        /// <summary>
        /// In meters (positive value)
        /// </summary>
        public double LowerDepth
        {
            get { return lowerDepth; }
            set
            {
                if (lowerDepth != value)
                {
                    lowerDepth = value;
                    RaisePropertyChanged(nameof(LowerDepth));
                    UpdateColumns();
                }
            }
        }

        private void UpdateColumns() {
            foreach (IColumn col in controlledColumns) {
                col.LowerDepth = LowerDepth;
                col.UpperDepth = UpperDepth;
                col.ColumnHeight = (LowerDepth - UpperDepth) * ScaleFactor;
            }
        }

        public void AttachToColumn(IColumn column) {
            controlledColumns.Add(column);
            column.LowerDepth = LowerDepth;
            column.UpperDepth = UpperDepth;
            column.ColumnHeight = (LowerDepth - UpperDepth) * ScaleFactor;
        }
    }
}
