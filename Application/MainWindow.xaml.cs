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
            persister.LoadProject();

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
                var regions = vm.CurrentProjectVM.BoreIntervalsVM.Intervals.SelectMany(i => ((Intervals.PhotoCalibratedBoreIntervalVM)i).GetRegionImages()).ToArray();

                double upperBoundary = vm.CurrentProjectVM.BoreIntervalsVM.Intervals.Where(i => !double.IsNaN(i.UpperDepth) && !double.IsNaN(i.LowerDepth)).Select(i => i.UpperDepth).Min();
                double lowerBoundary = vm.CurrentProjectVM.BoreIntervalsVM.Intervals.Where(i => !double.IsNaN(i.UpperDepth) && !double.IsNaN(i.LowerDepth)).Select(i => i.LowerDepth).Max();                

                vm.CurrentProjectVM.PlaneVM = PlaneHalpers.BuildPlane(
                    new LayersAnnotation() {
                        LayerBoundaries = new double[] { upperBoundary,lowerBoundary},
                        LayerAnnotation = new Dictionary<string, string[]>[] { new Dictionary<string, string[]>()}
                    },
                    vm.CurrentProjectVM.ActiveLayerTemplate,
                    vm.CurrentProjectVM.PlaneColumnSettingsVM,
                    regions);
                
                vm.CurrentProjectVM.PlaneVM.ActivateSettingsCommand = new DelegateCommand(() => {
                    vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneColumnSettingsVM;
                });
                
                vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneVM;
            });

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
                    catch (System.IO.FileNotFoundException) {
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
