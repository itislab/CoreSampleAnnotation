using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation
{
    public class MainWindowVM : ViewModel
    {
        private ProjectVM currentProjectVM = new ProjectVM();
        private ViewModel activeSectionVM = null;

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
    }
}
