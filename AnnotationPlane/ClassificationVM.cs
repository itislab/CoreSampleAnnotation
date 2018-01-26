using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnotationPlane
{
    class ClassificationVM : ViewModel
    {
        private ClassificationLayerVM layerVM;
        public ClassificationLayerVM LayerVM {
            get { return layerVM; }
            set {
                if (layerVM != value) {
                    layerVM = value;
                    RaisePropertyChanged(nameof(LayerVM));
                }
            }
        }

        private ICommand closeCommand;
        public ICommand CloseCommand {
            get { return closeCommand; }
            set {
                if (closeCommand != value) {
                    closeCommand = value;
                    RaisePropertyChanged(nameof(CloseCommand));
                }
            }
        }

        private ICommand classSelectedCommand;
        public ICommand ClassSelectedCommand {
            get { return classSelectedCommand; }
            set {
                if (classSelectedCommand != value)
                {
                    classSelectedCommand = value;
                    RaisePropertyChanged(nameof(ClassSelectedCommand));
                }
            }
        }

        private bool isVisisble;
        public bool IsVisible {
            get { return isVisisble; }
            set {
                if (isVisisble != value) {
                    isVisisble = value;
                    RaisePropertyChanged(nameof(IsVisible));
                }
            }
        }
    }
}
