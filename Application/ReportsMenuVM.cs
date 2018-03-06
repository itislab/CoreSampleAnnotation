using CoreSampleAnnotation.AnnotationPlane;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation {
    public class ReportsMenuVM : ViewModel {
        private ProjectVM projectVM;

        private ICommand openSamplesCSVDialogCommand = null;
        public ICommand OpenSamplesCSVDialogCommand {
            get {
                return openSamplesCSVDialogCommand;
            }
            set {
                if (openSamplesCSVDialogCommand != value) {
                    openSamplesCSVDialogCommand = value;
                    RaisePropertyChanged(nameof(OpenSamplesCSVDialogCommand));
                }
            }
        }

        public ProjectVM ProjectVM {
            get {
                return projectVM;
            }
        }

        public ReportsMenuVM(ProjectVM project) {//, ObservableCollection<ColumnVM> columns
            projectVM = project;
        }
    }
}
