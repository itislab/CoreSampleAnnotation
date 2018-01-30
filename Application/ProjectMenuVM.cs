using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation
{
    public class ProjectMenuVM : ViewModel
    {
        private ProjectVM projectVM;
        private ICommand activateBoreIntervalsCommand = null;
        private ICommand activateAnnotationPlane = null;
        private ICommand activateTemplateEditor = null;
        private ICommand activateReportGeneration = null;
        private ICommand exit = null;

        public ProjectVM ProjectVM
        {
            get
            {
                return projectVM;
            }
        }

        public ICommand ActivateBoreIntervalsCommand
        {
            get
            {
                return activateBoreIntervalsCommand;
            }
            set
            {
                if (activateBoreIntervalsCommand != value)
                {
                    activateBoreIntervalsCommand = value;
                    RaisePropertyChanged(nameof(ActivateBoreIntervalsCommand));
                }
            }
        }
        public ICommand ActivateAnnotationPlaneCommand
        {
            get
            {
                return activateAnnotationPlane;
            }
            set
            {
                if (activateAnnotationPlane != value)
                {
                    activateAnnotationPlane = value;
                    RaisePropertyChanged(nameof(ActivateAnnotationPlaneCommand));
                }
            }
        }
        public ICommand ActivateTemplateEditorCommand
        {
            get
            {
                return activateTemplateEditor;
            }
            set
            {
                if (activateTemplateEditor != value)
                {
                    activateTemplateEditor = value;
                    RaisePropertyChanged(nameof(ActivateTemplateEditorCommand));
                }
            }
        }
        public ICommand ActivateReportGenerationCommand
        {
            get
            {
                return activateReportGeneration;
            }
            set
            {
                if (activateReportGeneration != value)
                {
                    activateReportGeneration = value;
                    RaisePropertyChanged(nameof(ActivateReportGenerationCommand));
                }
            }
        }
        public ICommand ExitAppCommand
        {
            get { return exit; }
            set
            {
                if (exit != value)
                {
                    exit = value;
                    RaisePropertyChanged(nameof(ExitAppCommand));
                }
            }
        }

        public ProjectMenuVM(ProjectVM project)
        {
            projectVM = project;
        }


    }
}
