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
        private MainWindowVM vm = new MainWindowVM();
        private ProjectMenuVM menuVM = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
            
            vm.CurrentProjectVM.BoreIntervalsVM.ActivateIntervalImagesCommand = 
                new DelegateCommand(arg =>
                {
                    Intervals.PhotoCalibratedBoreIntervalVM intervalVM = arg as Intervals.PhotoCalibratedBoreIntervalVM;
                    vm.ActiveSectionVM = intervalVM;
                });

            menuVM = new ProjectMenuVM(vm.CurrentProjectVM);

            menuVM.ExitAppCommand = new DelegateCommand(() =>
            {
                var result = MessageBox.Show("Закрыть приложение?", "Выход", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    Close();
                }
            });

            menuVM.ActivateBoreIntervalsCommand = new DelegateCommand(() =>
            {
                vm.ActiveSectionVM = vm.CurrentProjectVM.BoreIntervalsVM;
            });

            menuVM.ActivateAnnotationPlaneCommand = new DelegateCommand(() =>
            {
                vm.ActiveSectionVM = vm.CurrentProjectVM.PlaneVM;
            });

            menuVM.ActivateReportGenerationCommand = new DelegateCommand(obj => { }, obj => false);
            menuVM.ActivateTemplateEditorCommand = new DelegateCommand(obj => { }, obj => false);

            vm.ActiveSectionVM = menuVM;

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
            if (value != null)
            {
                if (value is ProjectMenuVM)
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
