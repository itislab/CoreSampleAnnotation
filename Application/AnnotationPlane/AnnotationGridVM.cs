using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane
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
        
    }
}
