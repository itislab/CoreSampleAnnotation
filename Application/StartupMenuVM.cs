using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation
{
    public class StartupMenuVM : ViewModel
    {
        private ICommand newProjectCommand = null;
        public ICommand NewProjectCommand {
            get { return newProjectCommand; }
            set {
                if (newProjectCommand != value) {
                    newProjectCommand = value;
                    RaisePropertyChanged(nameof(NewProjectCommand));
                }
            }
        }

        private ICommand loadProjectCommand = null;
        public ICommand LoadProjectCommand {
            get { return loadProjectCommand; }
            set {
                if (loadProjectCommand != value) {
                    loadProjectCommand = value;
                    RaisePropertyChanged(nameof(LoadProjectCommand));
                }
            }
        }

        private ICommand exitAppCommand = null;
        public ICommand ExitAppCommand {
            get { return exitAppCommand; }
            set {
                if (exitAppCommand != value) {
                    exitAppCommand = value;
                    RaisePropertyChanged(nameof(ExitAppCommand));
                }
            }
        }
    }
}
