using CoreSampleAnnotation.AnnotationPlane.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class PlaneVM : ViewModel
    {
        private ColumnScale.Controller colScaleController = new ColumnScale.Controller();
        public ColumnScale.Controller ColScaleController
        {
            get
            {
                return colScaleController;
            }
        }

        private LayerSyncronization.Controller layerSyncController = new LayerSyncronization.Controller();
        public LayerSyncronization.Controller LayerSyncController
        {
            get
            {
                return layerSyncController;
            }

        }

        public AnnotationGridVM AnnoGridVM { private set; get; }
        public ClassificationVM classificationVM { private set; get; }

        private ICommand saveImageCommand;

        public ICommand SaveImageCommand {
            get { return saveImageCommand; }
            set {
                if (saveImageCommand != value) {
                    saveImageCommand = value;
                    RaisePropertyChanged(nameof(SaveImageCommand));
                }
            }
        }


        public ICommand PointSelected { private set; get; }

        private ICommand activateSettingsCommand;
        public ICommand ActivateSettingsCommand {
            get { return activateSettingsCommand; }
            set {
                if (activateSettingsCommand != value) {
                    activateSettingsCommand = value;
                    RaisePropertyChanged(nameof(ActivateSettingsCommand));
                }
            }
        }

        /// <summary>
        /// How much is 1 real meter in WPF coordinates
        /// </summary>
        public double ScaleFactor
        {
            get
            {
                return ColScaleController.ScaleFactor;
            }
            set
            {
                if (value != ColScaleController.ScaleFactor)
                {
                    ColScaleController.ScaleFactor = value;
                    layerSyncController.ScaleFactor = value;
                }
            }
        }        

        public PlaneVM()
        {
            AnnoGridVM = new AnnotationGridVM();
            classificationVM = new ClassificationVM();

            PointSelected = new DelegateCommand(obj =>
            {
                PointSelectedEventArgs psea = obj as PointSelectedEventArgs;
                System.Diagnostics.Debug.WriteLine("Selected point with offset {0} in {1} column", psea.WpfTopOffset, psea.ColumnIdx);

                Type[] newLayerTypes = new Type[] { typeof(ImageColumnVM), typeof(DepthAxisColumnVM) };
                ColumnVM relatedVM = AnnoGridVM.Columns[psea.ColumnIdx];
                Type relatedVmType = relatedVM.GetType();
                if (newLayerTypes.Contains(relatedVmType))
                {
                    System.Diagnostics.Debug.WriteLine("Layer split requested");
                    layerSyncController.SplitLayer(psea.WpfTopOffset);
                }
                else if (typeof(LayeredPresentationColumnVM) == relatedVmType)
                {
                    LayeredPresentationColumnVM lpvm = (LayeredPresentationColumnVM)relatedVM;
                    int layerIdx = layerSyncController.GetLayerIndex(psea.WpfTopOffset);
                    ClassificationLayerPresentingVM clpmv = lpvm.Layers[layerIdx] as ClassificationLayerPresentingVM;
                    if (clpmv != null)
                    {
                        classificationVM.LayerVM = clpmv.ClassificationVM;
                        classificationVM.IsVisible = true;
                    }
                }
            });

            LayerSyncController.PropertyChanged += LayerSyncController_PropertyChanged;
            ColScaleController.PropertyChanged += ColScaleController_PropertyChanged;            

            ColScaleController.ScaleFactor = 3000.0;

            classificationVM = new ClassificationVM();
            classificationVM.CloseCommand = new DelegateCommand(() => classificationVM.IsVisible = false);
            classificationVM.ClassSelectedCommand = new DelegateCommand(sender =>
            {
                FrameworkElement fe = sender as FrameworkElement;
                LayerClassVM lc = (LayerClassVM)fe.DataContext;
                classificationVM.LayerVM.CurrentClass = lc;
            });

            classificationVM.IsVisible = false;            
        }

        private void ColScaleController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ColScaleController.LowerDepth):
                    LayerSyncController.LowerDepth = ColScaleController.LowerDepth;
                    break;
                case nameof(ColScaleController.UpperDepth):
                    LayerSyncController.UpperDepth = ColScaleController.UpperDepth;
                    break;
                case nameof(ColScaleController.ScaleFactor):
                    LayerSyncController.ScaleFactor = ColScaleController.ScaleFactor;
                    break;
            }
        }

        private void LayerSyncController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LayerSyncController.LowerDepth):
                    ColScaleController.LowerDepth = LayerSyncController.LowerDepth;
                    break;
                case nameof(LayerSyncController.UpperDepth):
                    ColScaleController.UpperDepth = LayerSyncController.UpperDepth;
                    break;
                case nameof(LayerSyncController.ScaleFactor):
                    ColScaleController.ScaleFactor = LayerSyncController.ScaleFactor;
                    break;
            }
        }
    }
}
