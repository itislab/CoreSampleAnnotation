﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    public interface IColumnSettingsPersistence {
        void Persist(ColumnDefinitionVM[] definitions);
        bool Load(out ColumnDefinitionVM[] definitions);
        bool LoadDefaults(out ColumnDefinitionVM[] definitions);
    }

    [Serializable]
    public class ColumnSettingsVM : ViewModel, ISerializable
    {
        private ILayersTemplateSource layersTemplateSource;
        private ILayerRankNamesSource layerRankNameSource;
        private IColumnSettingsPersistence colSettingPersistence;
        private ColumnDefinitionVM[] columnDefinitions;
        public ColumnDefinitionVM[] ColumnDefinitions
        {
            get { return columnDefinitions; }
            set
            {
                if (columnDefinitions != value)
                {
                    columnDefinitions = value;
                    RaisePropertyChanged(nameof(ColumnDefinitions));
                    RaisePropertyChanged(nameof(OrderedColumnDefinitions));
                    if (MoveColumnLeft != null)
                        MoveColumnLeft.RaiseCanExecuteChanged();
                    if (MoveColumnRight != null)
                        MoveColumnRight.RaiseCanExecuteChanged();
                }
            }
        }

        public ColumnDefinitionVM[] OrderedColumnDefinitions
        {
            get { return ColumnDefinitions.OrderBy(cd => cd.ColumnOrder).ToArray(); }
        }

        public ICommand AddDepthCommand { get; private set; }
        public ICommand AddPhotoCommand { get; private set; }
        public ICommand AddLayerLengthCommand { get; private set; }
        public ICommand AddLayerPropCommand { get; private set; }
        public ICommand AddLayerBoundsCommand { get; private set; }
        public ICommand AddLayerSamplesCommand { get; private set; }
        public ICommand AddVisualCommand { get; private set; }
        public ICommand AddIconsCommand { get; private set; }

        private ICommand activateAnnotationPlaneCommand;
        public ICommand ActivateAnnotationPlaneCommand {
            get { return activateAnnotationPlaneCommand; }
            set {
                if (activateAnnotationPlaneCommand != value) {
                    activateAnnotationPlaneCommand = value;
                    RaisePropertyChanged(nameof(ActivateAnnotationPlaneCommand));
                }
            }
        }

        private ICommand importFromFileCommand;
        public ICommand ImportFromFileCommnad {
            get { return importFromFileCommand; }
            private set {
                if (importFromFileCommand != value) {
                    importFromFileCommand = value;
                    RaisePropertyChanged(nameof(ImportFromFileCommnad));
                }
            }
        }

        private ICommand exportToFileCommand;
        public ICommand ExportToFileCommand {
            get { return exportToFileCommand; }
            private set {
                if (exportToFileCommand != value) {
                    exportToFileCommand = value;
                    RaisePropertyChanged(nameof(ExportToFileCommand));
                }
            }
        }        

        private DelegateCommand MoveColumnLeft;
        private DelegateCommand MoveColumnRight;

        /// <summary>
        /// Sets the commands, column order and event handlers.
        /// After this the VM is ready for operation
        /// </summary>
        /// <param name="column"></param>
        private void InitializeColumn(ColumnDefinitionVM column)
        {

            int nonZeroOrderColumns = ColumnDefinitions.Count(c => c.ColumnOrder > 0);
            if (column.ColumnOrder == 0)
                column.ColumnOrder = nonZeroOrderColumns + 1;

            column.RemoveCommand = new DelegateCommand((parameter) =>
            {
                int order = (int)parameter;

                List<ColumnDefinitionVM> higherOrder = ColumnDefinitions.Where(cd => cd.ColumnOrder > order).ToList();
                List<ColumnDefinitionVM> lowerOrder = ColumnDefinitions.Where(cd => cd.ColumnOrder < order).ToList();

                foreach (ColumnDefinitionVM cdVM in higherOrder)
                    cdVM.ColumnOrder--;

                ColumnDefinitions = lowerOrder.Concat(higherOrder).ToArray();
            });

            column.MoveLeft = MoveColumnLeft;
            column.MoveRight = MoveColumnRight;
            MoveColumnLeft.RaiseCanExecuteChanged();
            MoveColumnRight.RaiseCanExecuteChanged();
        }        

        public ColumnSettingsVM(
            ILayersTemplateSource layersTemplateSource,
            ILayerRankNamesSource layerRankNameSource,
            IColumnSettingsPersistence settingsPersister)
        {
            this.layersTemplateSource = layersTemplateSource;
            this.layerRankNameSource = layerRankNameSource;
            this.colSettingPersistence = settingsPersister;
            ColumnDefinitions = new ColumnDefinitionVM[0];
            ColumnDefinitionVM[] settings = null;            
            if (settingsPersister.LoadDefaults(out settings))
            {
                ColumnDefinitions = settings;
                Initialize();
                foreach (var col in columnDefinitions)
                    InitializeColumn(col);
            }
            else {
                //default column set
                Initialize();
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>();
                var ranks = layerRankNameSource.InstrumentalMultipleNames.Reverse().ToArray();
                foreach (string rank in ranks)
                {
                    LayerEditColumnDefinitionVM column = new LayerEditColumnDefinitionVM(layerRankNameSource);
                    column.Selected = rank;
                    result.Add(column);
                    ColumnDefinitions = result.ToArray(); // As InitializeColumn uses ColumnDefinitions
                    InitializeColumn(column);
                }

                AddDepthCommand.Execute(null);
                AddPhotoCommand.Execute(null);
            }            
        }

        protected virtual void Initialize() {
            AddDepthCommand = new DelegateCommand(() =>
            {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new DepthColumnDefinitionVM();
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddLayerLengthCommand = new DelegateCommand(() =>
            {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new LayerLengthColumnDefinitionVM();
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddPhotoCommand = new DelegateCommand(() =>
            {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new PhotoColumnDefinitionVM();
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddLayerPropCommand = new DelegateCommand(() =>
            {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new LayeredTextColumnDefinitionVM(layersTemplateSource);
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddLayerBoundsCommand = new DelegateCommand(() => {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new LayerEditColumnDefinitionVM(layerRankNameSource);
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddLayerSamplesCommand = new DelegateCommand(() => {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new LayerSamplesDefinitionVM();
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddVisualCommand = new DelegateCommand(() => {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new VisualColumnDefinitionVM(layersTemplateSource);
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            AddIconsCommand = new DelegateCommand(() => {
                List<ColumnDefinitionVM> result = new List<ColumnDefinitionVM>(ColumnDefinitions);
                ColumnDefinitionVM column = new IconsColumnDefinitionVM(layersTemplateSource);
                InitializeColumn(column);
                result.Add(column);
                ColumnDefinitions = result.ToArray();
            });

            MoveColumnLeft = new DelegateCommand((parameter) =>
            {
                int order = (int)parameter;

                ColumnDefinitionVM target = ColumnDefinitions.Where(cd => cd.ColumnOrder == order).Single();
                ColumnDefinitionVM toSwap = ColumnDefinitions.Where(cd => cd.ColumnOrder == order - 1).Single();
                target.ColumnOrder = order - 1;
                toSwap.ColumnOrder = order;
                RaisePropertyChanged(nameof(ColumnDefinitions));
                RaisePropertyChanged(nameof(OrderedColumnDefinitions));
                MoveColumnLeft.RaiseCanExecuteChanged();
                MoveColumnRight.RaiseCanExecuteChanged();

            }, (parameter) =>
            {
                if (parameter != null)
                {
                    int order = (int)parameter;
                    if (order > 1)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            });

            MoveColumnRight = new DelegateCommand((parameter) =>
            {
                int order = (int)parameter;

                ColumnDefinitionVM target = ColumnDefinitions.Where(cd => cd.ColumnOrder == order).Single();
                ColumnDefinitionVM toSwap = ColumnDefinitions.Where(cd => cd.ColumnOrder == order + 1).Single();
                target.ColumnOrder = order + 1;
                toSwap.ColumnOrder = order;
                RaisePropertyChanged(nameof(ColumnDefinitions));
                RaisePropertyChanged(nameof(OrderedColumnDefinitions));
                MoveColumnLeft.RaiseCanExecuteChanged();
                MoveColumnRight.RaiseCanExecuteChanged();

            }, (parameter) =>
            {
                if (parameter != null)
                {
                    int order = (int)parameter;
                    if (order < ColumnDefinitions.Length)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            });
            ImportFromFileCommnad = new DelegateCommand(() =>
            {
                ColumnDefinitionVM[] settings;
                if (colSettingPersistence.Load(out settings))
                {
                    ColumnDefinitions = settings;
                    foreach (var col in ColumnDefinitions)
                        InitializeColumn(col);
                }
            });
            ExportToFileCommand = new DelegateCommand(() => {
                colSettingPersistence.Persist(OrderedColumnDefinitions);
            });

           
        }

        #region Serialization
        protected ColumnSettingsVM(SerializationInfo info, StreamingContext context) {
            layersTemplateSource = (ILayersTemplateSource)info.GetValue("LayersTemplateSource", typeof(ILayersTemplateSource));
            layerRankNameSource = (ILayerRankNamesSource)info.GetValue("LayerRankNameSource", typeof(ILayerRankNamesSource));
            colSettingPersistence = (IColumnSettingsPersistence)info.GetValue("SettingsPersister",typeof(IColumnSettingsPersistence));
            columnDefinitions = (ColumnDefinitionVM[])info.GetValue("Columns",typeof(ColumnDefinitionVM[]));            
            Initialize();
            foreach (var col in columnDefinitions)
                InitializeColumn(col);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LayersTemplateSource", layersTemplateSource);
            info.AddValue("LayerRankNameSource", layerRankNameSource);
            info.AddValue("Columns", OrderedColumnDefinitions);
            info.AddValue("SettingsPersister",colSettingPersistence);
        }
        #endregion

    }
}
