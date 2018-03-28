using CoreSampleAnnotation.AnnotationPlane.ColumnSettings;
using CoreSampleAnnotation.AnnotationPlane.Template;
using CoreSampleAnnotation.Intervals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;

namespace CoreSampleAnnotation
{
    public enum AnnotationDirection {
        UpToBottom,
        BottomToUp
    }

    public class BoolToBrushConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; }
        public Brush FalseBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if ((bool)value)
                return TrueBrush;
            else
                return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class ProjectVM : ViewModel, ISerializable
    {
        
        private ILayerRankNamesSource layerRankNameSource;
        public ILayerRankNamesSource LayerRankNameSource {
            get
            { return layerRankNameSource; }
            set {
                if (layerRankNameSource != value) {
                    layerRankNameSource = value;
                    RaisePropertyChanged(nameof(LayerRankNameSource));
                }
            }
        }

        private IColumnSettingsPersistence columnSettingsPersistence;
        public IColumnSettingsPersistence ColumnSettingsPersistence {
            get {
                return columnSettingsPersistence;
            }
            set {
                if (columnSettingsPersistence != value) {
                    columnSettingsPersistence = value;
                    RaisePropertyChanged(nameof(ColumnSettingsPersistence));
                }
            }
        }

        private string boreName = string.Empty;
        /// <summary>
        /// The name og the bore hole from where the core samples are extracted
        /// </summary>
        public string BoreName {
            get {
                return boreName;
            }
            set {
                if (boreName != value) {
                    boreName = value;
                    RaisePropertyChanged(nameof(BoreName));
                }
            }
        }

        
        public bool IsUpToBottomLayerNumbering { get { return AnnotationDirection == AnnotationDirection.UpToBottom; } }
        public bool IsBottomToUpLayerNumbering { get { return AnnotationDirection == AnnotationDirection.BottomToUp; } }

        public ICommand ActivateUpToBottomNumberingCommand { get; private set; }
        
        public ICommand ActivateBottomToUpNumberingCommand { get; private set; }

        private ILayersTemplateSource layersTemplateSource;

        public ILayersTemplateSource LayersTemplateSource
        {
            get { return layersTemplateSource; }
            set
            {
                if (layersTemplateSource != value)
                {
                    layersTemplateSource = value;
                    RaisePropertyChanged(nameof(LayersTemplateSource));
                }
            }
        }        

        private BoreIntervalsVM boreIntervalsVM;
        public BoreIntervalsVM BoreIntervalsVM {
            get { return boreIntervalsVM; }
            set {
                if (boreIntervalsVM != value) {
                    boreIntervalsVM = value;
                    RaisePropertyChanged(nameof(BoreIntervalsVM));
                }
            }
        }

        private AnnotationDirection annotationDirection;
        /// <summary>
        /// The direction in which the layers are numbered.        
        /// </summary>
        public AnnotationDirection AnnotationDirection
        {
            get { return annotationDirection; }
            set
            {
                if (annotationDirection != value)
                {
                    annotationDirection = value;
                    RaisePropertyChanged(nameof(AnnotationDirection));
                    RaisePropertyChanged(nameof(IsUpToBottomLayerNumbering));
                    RaisePropertyChanged(nameof(IsBottomToUpLayerNumbering));
                    if (PlaneVM != null)
                        PlaneVM.AnnotationDirection = value;
                }
            }
        }

        private AnnotationPlane.PlaneVM planeVM = null;

        public AnnotationPlane.PlaneVM PlaneVM {
            get {
                return planeVM;
            }
            set {
                if (value != planeVM) {
                    if (planeVM != null) {
                        planeVM.PropertyChanged -= PlaneVM_PropertyChanged;
                    }
                    planeVM = value;

                    planeVM.PropertyChanged += PlaneVM_PropertyChanged;

                    if (planeVM != null)
                        AnnotationDirection = PlaneVM.AnnotationDirection;
                    RaisePropertyChanged(nameof(PlaneVM));
                }
            }
        }

        private ColumnSettingsVM planeColumnSettingsVM;

        public ColumnSettingsVM PlaneColumnSettingsVM {
            get { return planeColumnSettingsVM; }
            set {
                if (planeColumnSettingsVM != value) {
                    planeColumnSettingsVM = value;
                    RaisePropertyChanged(nameof(PlaneColumnSettingsVM));
                }
            }
        }

        public void Initialize() {
            ActivateBottomToUpNumberingCommand = new DelegateCommand(() => { AnnotationDirection = AnnotationDirection.BottomToUp; });
            ActivateUpToBottomNumberingCommand = new DelegateCommand(() => { AnnotationDirection = AnnotationDirection.UpToBottom; });
            if(planeColumnSettingsVM == null)
                planeColumnSettingsVM = new ColumnSettingsVM(LayersTemplateSource,LayerRankNameSource,ColumnSettingsPersistence);            
        }

        private void PlaneVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(PlaneVM.AnnotationDirection):
                    AnnotationDirection = PlaneVM.AnnotationDirection;
                    RaisePropertyChanged(nameof(IsUpToBottomLayerNumbering));
                    RaisePropertyChanged(nameof(IsBottomToUpLayerNumbering));
                    break;
            }
        }

        public ProjectVM(
            IImageStorage imageStorage,
            ILayersTemplateSource layersTemplateSource,
            ILayerRankNamesSource layerRankSource,
            IColumnSettingsPersistence columnSettingsPersister) {            
            boreIntervalsVM = new BoreIntervalsVM(imageStorage);
            this.layerRankNameSource = layerRankSource;
            this.layersTemplateSource = layersTemplateSource;
            columnSettingsPersistence = columnSettingsPersister;            

            Initialize();
        }

        #region Serialization

        protected ProjectVM(SerializationInfo info, StreamingContext context) {
            boreName = info.GetString("BoreName");
            boreIntervalsVM = (BoreIntervalsVM)info.GetValue("Intervals", typeof(BoreIntervalsVM));
            PlaneVM = (AnnotationPlane.PlaneVM)info.GetValue("Annotation", typeof(AnnotationPlane.PlaneVM));
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource",typeof(ILayersTemplateSource));
            layerRankNameSource = (ILayerRankNamesSource)info.GetValue("LayersRankSource", typeof(ILayerRankNamesSource));
            columnSettingsPersistence = (IColumnSettingsPersistence)info.GetValue("ColumnValuePersistence",typeof(IColumnSettingsPersistence));
            planeColumnSettingsVM = (ColumnSettingsVM)info.GetValue("ColumnsSettings", typeof(ColumnSettingsVM));
            if(PlaneVM != null)
                AnnotationDirection = PlaneVM.AnnotationDirection;         
            Initialize();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("BoreName", BoreName);
            info.AddValue("Intervals", BoreIntervalsVM);
            info.AddValue("Annotation",PlaneVM);
            info.AddValue("LayersTemplateSource", LayersTemplateSource);
            info.AddValue("LayersRankSource", LayerRankNameSource);
            info.AddValue("ColumnValuePersistence",ColumnSettingsPersistence);
            info.AddValue("ColumnsSettings", PlaneColumnSettingsVM);
        }
        #endregion
    }
}
