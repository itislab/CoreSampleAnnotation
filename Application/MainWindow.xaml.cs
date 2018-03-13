using CoreSampleAnnotation.AnnotationPlane;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoreSampleAnnotation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM vm = new MainWindowVM(new Persistence.FolderPersisterFactory());
        private ProjectMenuVM menuVM = null;

        private readonly DelegateCommand ExitCommand;

        private void LoadProjectWithPersister(IProjectPersister persister)
        {
            vm.ActivePersister = persister;

            vm.CurrentProjectVM = persister.LoadProject();
            menuVM = new ProjectMenuVM(vm.CurrentProjectVM);
            vm.ActiveSectionVM = menuVM;

            vm.CurrentProjectVM.BoreIntervalsVM.ActivateIntervalImagesCommand =
                new DelegateCommand(arg =>
                {
                    Intervals.PhotoCalibratedBoreIntervalVM intervalVM = arg as Intervals.PhotoCalibratedBoreIntervalVM;
                    vm.ActiveSectionVM = intervalVM;
                });


            menuVM.ActivateBoreIntervalsCommand = new DelegateCommand(() =>
            {
                vm.ActiveSectionVM = vm.CurrentProjectVM.BoreIntervalsVM;
            });

            menuVM.ExitAppCommand = ExitCommand;

            menuVM.SaveCommand = new DelegateCommand(() =>
            {
                vm.ActivePersister.SaveProject(vm.CurrentProjectVM);
                MessageBox.Show("Проект успешно сохранен", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            menuVM.ActivateAnnotationPlaneCommand = new DelegateCommand(() =>
            {
                var intervals = vm.CurrentProjectVM.BoreIntervalsVM.Intervals.
                    Select(i => (Intervals.PhotoCalibratedBoreIntervalVM)i).
                    Where(i => !double.IsNaN(i.UpperDepth) && !double.IsNaN(i.LowerDepth) && !double.IsNaN(i.ExtractedLength)).ToArray();
                var regions = intervals.SelectMany(i => i.GetRegionImages()).ToArray();
                foreach (var interval in intervals)
                {
                    if (interval.AnnotatedPercentage < 100.0)
                    {
                        var res = MessageBox.Show(string.Format("В интервале {0:0.##} - {1:0.##} м на фото отмечен не весь измеренный выход (только {2:0.##}% отмечено). Действительно приступить к описанию слоев?", interval.UpperDepth, interval.LowerDepth, interval.AnnotatedPercentage), "Не закончена работа с фото", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                        switch (res)
                        {
                            case MessageBoxResult.Yes: break;
                            case MessageBoxResult.No:
                                return;
                        }
                    }
                }

                double upperBoundary = intervals.Select(i => i.UpperDepth).Min();
                double lowerBoundary = intervals.Select(i => i.UpperDepth + i.ExtractedLength).Max();

                if (vm.CurrentProjectVM.PlaneVM == null)
                {
                    LayersAnnotation emptyAnnotation = new LayersAnnotation();
                    emptyAnnotation.LayerBoundaries = new double[] { upperBoundary, lowerBoundary };
                    emptyAnnotation.Columns = new ColumnValues[0];

                    vm.CurrentProjectVM.PlaneVM = new PlaneVM(emptyAnnotation, vm.CurrentProjectVM.LayersTemplateSource);
                }
                else
                {
                    //check whether the intervals boundaries change
                    LayersAnnotation layersAnnotation = vm.CurrentProjectVM.PlaneVM.AsLayersAnnotation;
                    double[] boundaries = layersAnnotation.LayerBoundaries;
                    bool boundariesChanged = false;
                    int upperLayersRemovedCount = 0;
                    //dealing with upper bound
                    if (upperBoundary < boundaries[0])
                    {
                        //the depth interval growed
                        boundaries[0] = upperBoundary;
                        boundariesChanged = true;
                    }
                    else if (upperBoundary > boundaries[0])
                    {
                        boundariesChanged = true;
                        int idx = Array.BinarySearch(boundaries, upperBoundary);
                        bool isExactMatch = true;
                        if (idx < 0)
                        {
                            idx = ~idx;
                            isExactMatch = false;
                        }

                        upperLayersRemovedCount = idx;
                        if (!isExactMatch)
                            upperLayersRemovedCount--;

                        foreach (var column in layersAnnotation.Columns)
                        {
                            column.LayerValues = column.LayerValues.Skip(upperLayersRemovedCount).ToArray();
                        }
                        boundaries = boundaries.Skip(upperLayersRemovedCount).ToArray();
                        if (!isExactMatch)
                            boundaries[0] = upperBoundary;
                    }
                    if (upperLayersRemovedCount > 0)
                        MessageBox.Show(string.Format("{0} верхних слоев были удалены, так как соответствующие им глубины больше не входят в указанные интервалы отбора. Если это не то, что Вы ожидали, не сохраняйте проект на диск.", upperLayersRemovedCount), "Потеря информации о слоях", MessageBoxButton.OK, MessageBoxImage.Warning);

                    int lowerLayersRemovedCount = 0;
                    //dealing with lower bound
                    if (lowerBoundary > boundaries[boundaries.Length - 1])
                    {
                        //the depth interval growed to the bottom
                        boundaries[boundaries.Length - 1] = lowerBoundary;
                        boundariesChanged = true;
                    }
                    else if (lowerBoundary < boundaries[boundaries.Length - 1])
                    {
                        boundariesChanged = true;
                        int idx = Array.BinarySearch(boundaries, lowerBoundary);
                        bool isExactMatch = true;
                        if (idx < 0)
                        {
                            idx = ~idx;
                            isExactMatch = false;
                        }
                        int layersToTake = idx;
                        lowerLayersRemovedCount = boundaries.Length - 1 - layersToTake;
                        foreach (var column in layersAnnotation.Columns)
                        {
                            column.LayerValues = column.LayerValues.Take(layersToTake).ToArray();
                        }
                        boundaries = boundaries.Take(layersToTake + 1).ToArray();
                        if (!isExactMatch)
                            boundaries[boundaries.Length - 1] = lowerBoundary;
                    }

                    if (lowerLayersRemovedCount > 0)
                        MessageBox.Show(string.Format("{0} нижних слоев были удалены, так как соответствующие им глубины больше не входят в указанные интервалы отбора. Если это не то, что Вы ожидали, не сохраняйте проект на диск.", lowerLayersRemovedCount), "Потеря информации о слоях", MessageBoxButton.OK, MessageBoxImage.Warning);

                    int samplesToRemove = vm.CurrentProjectVM.PlaneVM.SamplesColumnVM.Samples.Count(s => s.Depth < upperBoundary || s.Depth > lowerBoundary);
                    double[] validSamples = vm.CurrentProjectVM.PlaneVM.SamplesColumnVM.Samples.Select(s => s.Depth).Where(s => s >= upperBoundary && s <= lowerBoundary).ToArray();

                    if (samplesToRemove > 0)
                        MessageBox.Show(string.Format("{0} точек взятия образцов были удалены, так как соответствующие им глубины больше не входят в указанные интервалы отбора. Если это не то, что Вы ожидали, не сохраняйте проект на диск.", samplesToRemove), "Потеря информации о слоях", MessageBoxButton.OK, MessageBoxImage.Warning);

                    if (boundariesChanged)
                    {
                        //recreating VM with new corrected depth bounds
                        layersAnnotation.LayerBoundaries = boundaries;
                        vm.CurrentProjectVM.PlaneVM = new PlaneVM(layersAnnotation, vm.CurrentProjectVM.LayersTemplateSource);
                        vm.CurrentProjectVM.PlaneVM.SamplesColumnVM.Samples = validSamples.Select(s => new SampleVM(s)).ToArray();
                    }
                }

                vm.CurrentProjectVM.PlaneVM.SetPresentationColumns(vm.CurrentProjectVM.PlaneColumnSettingsVM,
                    regions);

                vm.CurrentProjectVM.PlaneVM.ActivateSettingsCommand = new DelegateCommand(() =>
                {
                    vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneColumnSettingsVM;
                });

                vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneVM;
            });

            vm.CurrentProjectVM.PlaneColumnSettingsVM.ActivateAnnotationPlaneCommand = menuVM.ActivateAnnotationPlaneCommand;

            menuVM.ActivateReportGenerationCommand = new DelegateCommand(obj => { }, obj => false);
            menuVM.ActivateTemplateEditorCommand = new DelegateCommand(obj => { }, obj => false);
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;

            ExitCommand = new DelegateCommand(() =>
            {
                if (vm.CurrentProjectVM == null) //there is not project loaded yet. nothing to save
                    Close();
                else
                {
                    var result = MessageBox.Show("Сохранить проект перед закрытием приложения?", "Сохранение при выходе", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            try
                            {
                                vm.ActivePersister.SaveProject(vm.CurrentProjectVM);
                                MessageBox.Show("Проект успешно сохранен", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                                Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("происошла ошибка сохранения проекта: \n" + ex.ToString(), "Ошибка сохранения проекта", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                        case MessageBoxResult.No:
                            Close();
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default: throw new InvalidOperationException();
                    }
                }
            });

            vm.StartupMenuVM.ExitAppCommand = ExitCommand;

            vm.StartupMenuVM.NewProjectCommand = new DelegateCommand(() =>
            {
                IProjectPersister createdPersister;

                if (vm.ProjectPersisterFactory.TryCreateNew(out createdPersister))
                {
                    LoadProjectWithPersister(createdPersister);
                }
            });

            vm.StartupMenuVM.LoadProjectCommand = new DelegateCommand(() =>
            {
                IProjectPersister restoredPersister;

                if (vm.ProjectPersisterFactory.TryRestoreExisting(out restoredPersister))
                {
                    try
                    {
                        LoadProjectWithPersister(restoredPersister);
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        MessageBox.Show("Выбранная вами папка не является папкой проекта описания скважины. Проверьте, правильную ли папку вы выбрали.", "Не папка проекта", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("происошла ошибка загрузки проекта: \n" + ex.ToString(), "Ошибка загрузки проекта", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            vm.ActiveSectionVM = vm.StartupMenuVM;

            Activate();
        }

        private void ButtonActivateMenu_Click(object sender, RoutedEventArgs e)
        {
            vm.ActiveSectionVM = menuVM;
        }
    }

    public class MenuButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type[] menuCollapsedTypes = new Type[] { typeof(ProjectMenuVM), typeof(StartupMenuVM) };

            if (value != null)
            {
                if (menuCollapsedTypes.Contains(value.GetType()))
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
