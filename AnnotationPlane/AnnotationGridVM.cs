using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationPlane
{
    public class AnnotationGridVM: ViewModel
    {
        private ObservableCollection<ColumnVM> columns = new ObservableCollection<ColumnVM>();

        public ObservableCollection<ColumnVM> Columns {
            get { return columns; }
            set {
                if (columns != value) {
                    columns = value;
                    RaisePropertyChanged(nameof(Columns));
                }
            }
        }

        private LayeredColumnVM secondColumn = new LayeredColumnVM("Можность эл-та цикла (м)");
        public LayeredColumnVM SecondColumn {
            get {
                return secondColumn;
            }
            set {
                if (secondColumn != value) {
                    secondColumn = value;
                    RaisePropertyChanged(nameof(SecondColumn));
                }
            }
        }
    }
}
