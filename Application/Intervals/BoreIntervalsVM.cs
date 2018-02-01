using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoreSampleAnnotation.Intervals
{
    public class BoreIntervalsVM : ViewModel
    {

        public ICommand AddNewCommand { private set; get; }

        public ICommand RemoveIntervalCommand { private set; get; }

        private ICommand activateIntervalImagesCommand = null;
        public ICommand ActivateIntervalImagesCommand {
            get { return activateIntervalImagesCommand; }
            set {
                if (activateIntervalImagesCommand != value) {
                    activateIntervalImagesCommand = value;
                    RaisePropertyChanged(nameof(ActivateIntervalImagesCommand));
                    RaisePropertyChanged(nameof(EffectiveActivateIntervalImagesCommand));
                }
            }
        }
        private DelegateCommand effectiveActivateIntervalImagesCommand = null;

        /// <summary>
        /// includes foreced additional checks for can exectue, and forced additional actions
        /// </summary>
        public ICommand EffectiveActivateIntervalImagesCommand {
            get {
                return effectiveActivateIntervalImagesCommand;
            }
        }
        
        private ObservableCollection<BoreIntervalVM> intervals = new ObservableCollection<BoreIntervalVM>();

        public ObservableCollection<BoreIntervalVM> Intervals {
            get { return intervals; }
            set {
                if (intervals != value) {
                    if (intervals != null) {
                        intervals.CollectionChanged -= Intervals_CollectionChanged;
                    }

                    intervals = value;
                    intervals.CollectionChanged += Intervals_CollectionChanged;

                    RaisePropertyChanged(nameof(Intervals));
                    RaisePropertyChanged(nameof(HasIntervals));
                }
            }
        }

        public bool HasIntervals {
            get { return Intervals.Count > 0; }
        }

        public BoreIntervalsVM() {
            effectiveActivateIntervalImagesCommand = new DelegateCommand(obj => {
                //activating the command that are set externally
                ActivateIntervalImagesCommand.Execute(obj);
                ///now performing additional actions
                PhotoCalibratedBoreIntervalVM vm = obj as PhotoCalibratedBoreIntervalVM;
                if (vm.ImagesCount == 0)
                    vm.AddNewImageCommand.Execute(null);
                }, obj => {
                    //checking extrernally set conditions 
                    bool externalCanExecute = ActivateIntervalImagesCommand.CanExecute(obj);
                    //checking local conditions
                    PhotoCalibratedBoreIntervalVM vm = obj as PhotoCalibratedBoreIntervalVM;
                    return externalCanExecute && !double.IsNaN(vm.UpperDepth) && !double.IsNaN(vm.LowerDepth) && (vm.LowerDepth > vm.UpperDepth);
                }
                );

            intervals.CollectionChanged += Intervals_CollectionChanged;

            Intervals.Add(new PhotoCalibratedBoreIntervalVM());

            AddNewCommand = new DelegateCommand(() => {
                Intervals.Add(new PhotoCalibratedBoreIntervalVM());
            });

            RemoveIntervalCommand = new DelegateCommand((arg) => {
                BoreIntervalVM biVM = arg as BoreIntervalVM;

                if (!(double.IsNaN(biVM.UpperDepth) || double.IsNaN(biVM.LowerDepth))) {
                    var result = MessageBox.Show(string.Format("Вы уверены, что хотите идалить интервал отбора {0} - {1}? Ассоциированные с ним фотографии керна будут удалены из проекта.", biVM.UpperDepth, biVM.LowerDepth), "Удаление интервала отбора", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.OK)
                    {
                        //System.Diagnostics.Debug.WriteLine("delete "+arg.ToString()+" request");
                        Intervals.Remove(biVM);
                    }
                }
                else
                    Intervals.Remove(biVM);              
            });
        }

        private void Intervals_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(HasIntervals));
            if (e.NewItems != null) {
                foreach (object obj in e.NewItems) {
                    BoreIntervalVM vm = obj as BoreIntervalVM;
                    vm.PropertyChanged += Vm_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (object obj in e.OldItems)
                {
                    BoreIntervalVM vm = obj as BoreIntervalVM;
                    vm.PropertyChanged -= Vm_PropertyChanged;
                }
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            BoreIntervalVM vm = sender as BoreIntervalVM;
            switch (e.PropertyName) {
                case nameof(vm.LowerDepth):
                case nameof(vm.UpperDepth):
                    effectiveActivateIntervalImagesCommand.RaiseCanExecuteChanged();
                    break;
            }
        }
    }
}
