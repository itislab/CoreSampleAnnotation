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

        private ICommand openTextReportDialogCommand = null;
        public ICommand OpenTextReportDialogCommand {
            get {
                return openTextReportDialogCommand;
            }
            set {
                if (openTextReportDialogCommand != value) {
                    openTextReportDialogCommand = value;
                    RaisePropertyChanged(nameof(OpenTextReportDialogCommand));
                }
            }
        }

        private ICommand openFullCSVDialogCommand = null;
        public ICommand OpenFullCSVDialogCommand {
            get {
                return openFullCSVDialogCommand;
            }
            set {
                if (openFullCSVDialogCommand != value) {
                    openFullCSVDialogCommand = value;
                    RaisePropertyChanged(nameof(OpenFullCSVDialogCommand));
                }
            }
        }

        public ProjectVM ProjectVM {
            get {
                return projectVM;
            }
        }

        public ReportsMenuVM(ProjectVM project) {
            projectVM = project;
        }
    }
}
