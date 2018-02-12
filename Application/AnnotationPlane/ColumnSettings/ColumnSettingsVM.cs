using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane.ColumnSettings
{
    public class ColumnSettingsVM : ViewModel
    {
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

        private DelegateCommand MoveColumnLeft;
        private DelegateCommand MoveColumnRight;

        /// <summary>
        /// Sets the commands, column order and event handlers.
        /// After this the VM is ready for operation
        /// </summary>
        /// <param name="column"></param>
        private void InitializeColumn(ColumnDefinitionVM column)
        {

            int columns = ColumnDefinitions.Length;
            if (column.ColumnOrder == 0)
                column.ColumnOrder = columns + 1;

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
        }

        public ColumnSettingsVM(Template.Property[] template)
        {
            ColumnDefinitions = new ColumnDefinitionVM[0];

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
                ColumnDefinitionVM column = new LayeredTextColumnDefinitionVM(template);
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
        }

    }
}
