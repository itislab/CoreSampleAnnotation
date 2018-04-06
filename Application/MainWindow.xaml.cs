using CoreSampleAnnotation.AnnotationPlane;
using Microsoft.Win32;
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

        private DelegateCommand ActivateAnnotationPlaneDelegateCommand;

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

            menuVM.ActivateAnnotationPlaneCommand = this.ActivateAnnotationPlaneDelegateCommand;

            vm.CurrentProjectVM.PlaneColumnSettingsVM.ActivateAnnotationPlaneCommand = menuVM.ActivateAnnotationPlaneCommand;            

            menuVM.ActivateReportGenerationCommand = new DelegateCommand(obj =>
            {
                ReportsMenuVM reportsMenuVM = new ReportsMenuVM(vm.CurrentProjectVM);
                reportsMenuVM.OpenSamplesCSVDialogCommand = new DelegateCommand(() =>
                {
                    SamplesColumnVM sampleColVM = vm.CurrentProjectVM.PlaneVM.SamplesColumnVM;
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.FileName = "образцы";
                    dlg.DefaultExt = ".csv";
                    dlg.Filter = "CSV files|*.csv";
                    bool? result = dlg.ShowDialog();
                    if (result == true)
                        Reports.SamplesCSV.Report.Generate(dlg.FileName, sampleColVM);
                    MessageBox.Show("Файл с образцами успешно сохранен","Успешно",MessageBoxButton.OK,MessageBoxImage.Information);
                });

                reportsMenuVM.OpenTextReportDialogCommand = new DelegateCommand(() =>
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.FileName = "Заготовка текстового отчета";
                    dlg.DefaultExt = ".rtf";
                    dlg.Filter = "Rich Text Format|*.rtf";
                    bool? result = dlg.ShowDialog();
                    if (result == true)
                    {
                        string[] rankNames = vm.CurrentProjectVM.LayerRankNameSource.NominativeNames;

                        //transforming boundaryVMs to report specific boundaries
                        
                        Reports.RTF.LayerBoundary[] boundaries =
                            vm.CurrentProjectVM.PlaneVM.LayerBoundaries
                                .Select(b =>
                                    new Reports.RTF.LayerBoundary(
                                        b.Numbers,
                                        vm.CurrentProjectVM.PlaneVM.LayerSyncController.WpfToDepth(b.Level),
                                        b.Rank
                                        )).ToArray();

                        //preparing RTF specific layer data
                        Reports.RTF.LayerDescrition[] layers = new Reports.RTF.LayerDescrition[boundaries.Length - 1];
                        var layersAnnotation = vm.CurrentProjectVM.PlaneVM.AsLayersAnnotation;

                        Dictionary<string, AnnotationPlane.Template.Property> propHelper = new Dictionary<string, AnnotationPlane.Template.Property>();
                        foreach (var prop in vm.CurrentProjectVM.LayersTemplateSource.Template)
                        {
                            propHelper.Add(prop.ID, prop);
                        }

                        Func<AnnotationPlane.Template.Class, string> texturalRepresentation =
                                    classValue =>
                                    {
                                        if (!string.IsNullOrEmpty(classValue.ShortName))
                                            return classValue.ShortName;
                                        if (!string.IsNullOrEmpty(classValue.Acronym))
                                            return classValue.Acronym;
                                        return classValue.ID;
                                    };

                        for (int i = 0; i < layers.Length; i++)
                        {
                            List<Reports.RTF.PropertyDescription> props = new List<Reports.RTF.PropertyDescription>();

                            for (int j = 0; j < layersAnnotation.Columns.Length; j++)
                            {
                                var col = layersAnnotation.Columns[j];
                                var value = col.LayerValues;
                                var prop1 = propHelper[col.PropID];

                                string[] values = null;
                                if (value[i].Value != null)
                                    values = value[i].Value.Select(v => texturalRepresentation(prop1.Classes.Single(c => c.ID == v))).ToArray();

                                Reports.RTF.PropertyDescription prop =
                                new Reports.RTF.PropertyDescription(
                                    prop1.Name,
                                    values,
                                    value[i].Remarks);
                                props.Add(prop);
                            }
                            layers[i] = new Reports.RTF.LayerDescrition(props.ToArray());
                        }

                        var samples = vm.CurrentProjectVM.PlaneVM.SamplesColumnVM.Samples
                            .Select(s => new Reports.RTF.Sample(
                                s.Number.ToString(),
                                vm.CurrentProjectVM.PlaneVM.LayerSyncController.WpfToDepth(s.Level),
                                s.Comment)).ToArray();

                        Reports.RTF.ReportTable table =
                            Reports.RTF.ReportHelpers.GenerateTableContents(
                                vm.CurrentProjectVM.BoreIntervalsVM.Intervals
                                    .Where(i => !double.IsNaN(i.UpperDepth) && !double.IsNaN(i.LowerDepth))
                                    .ToArray(),
                                boundaries,
                                layers,
                                rankNames,
                                samples,
                                vm.CurrentProjectVM.AnnotationDirection
                                );

                        Reports.RTF.TableDocument.FormRTFDocument(dlg.FileName,table);
                        MessageBox.Show("Файл с заготовкой текстового отчета успешно сохранен", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });

                vm.ActiveSectionVM = reportsMenuVM;
            });
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

            ActivateAnnotationPlaneDelegateCommand = new DelegateCommand((obj) =>
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

                    vm.CurrentProjectVM.PlaneVM = new PlaneVM(emptyAnnotation, vm.CurrentProjectVM.LayersTemplateSource, vm.CurrentProjectVM.LayerRankNameSource);
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
                        vm.CurrentProjectVM.PlaneVM = new PlaneVM(layersAnnotation, vm.CurrentProjectVM.LayersTemplateSource, vm.CurrentProjectVM.LayerRankNameSource);
                        vm.CurrentProjectVM.PlaneVM.SamplesColumnVM.Samples = validSamples.Select(s => new SampleVM(s)).ToArray();
                    }
                }

                vm.CurrentProjectVM.PlaneVM.SetPresentationColumns(vm.CurrentProjectVM.PlaneColumnSettingsVM,
                    regions);

                vm.CurrentProjectVM.PlaneVM.ActivateSettingsCommand = new DelegateCommand(() =>
                {
                    vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneColumnSettingsVM;
                });

                vm.CurrentProjectVM.PlaneVM.SaveProjectCommand = menuVM.SaveCommand;

                vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneVM;
            }, (obj) =>
            {
                var intervals = vm.CurrentProjectVM.BoreIntervalsVM.Intervals.
                    Select(i => (Intervals.PhotoCalibratedBoreIntervalVM)i).
                    Where(i => !double.IsNaN(i.UpperDepth) && !double.IsNaN(i.LowerDepth) && !double.IsNaN(i.ExtractedLength)).ToArray();
                return intervals.Length > 0;
            });

            Activate();
        }

        private void ButtonActivateMenu_Click(object sender, RoutedEventArgs e)
        {
            vm.ActiveSectionVM = menuVM;
            ActivateAnnotationPlaneDelegateCommand.RaiseCanExecuteChanged();
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
