using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation
{
    /// <summary>
    /// Load and saves the project into some persistent location
    /// </summary>
    public interface IProjectPersister {
        ProjectVM LoadProject();
        void SaveProject(ProjectVM project);        
    }

    public interface IProjectPersisterFactory {
        bool TryCreateNew(out IProjectPersister persister);
        bool TryRestoreExisting(out IProjectPersister persister);
    }

    public class MainWindowVM : ViewModel
    {
        private ProjectVM currentProjectVM = null;        
        private StartupMenuVM startupVM = new StartupMenuVM();
        private ViewModel activeSectionVM = null;
        public IProjectPersister ActivePersister { get; set; }
        public IProjectPersisterFactory ProjectPersisterFactory { get; private set; }

        /// <summary>
        /// When this is null, the section chooser occupies the screen, when it is not null, the corresponding section view occupies the screen
        /// </summary>
        public ViewModel ActiveSectionVM {
            get { return activeSectionVM; }
            set {
                if (activeSectionVM != value) {
                    activeSectionVM = value;
                    RaisePropertyChanged(nameof(ActiveSectionVM));
                }
            }
        }

        /// <summary>
        /// A currently opened project
        /// </summary>
        public ProjectVM CurrentProjectVM {
            get { return currentProjectVM; }
            set {
                if (currentProjectVM != value) {
                    currentProjectVM = value;
                    RaisePropertyChanged(nameof(CurrentProjectVM));
                }
            }
        }

        public StartupMenuVM StartupMenuVM {
            get { return startupVM; }
        }

        public MainWindowVM(IProjectPersisterFactory projectPersisterFactory) {
            this.ProjectPersisterFactory = projectPersisterFactory;
        }
    }
}
