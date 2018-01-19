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

namespace All
{   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExtractionInfoVM extractionViewVM;
        private PhotoMarkupVM photoMarkupVM;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            photoMarkupVM = new PhotoMarkupVM();
            photoMarkupVM.ImagePath = "kern.jpg";
            //propagating changes to other VMs
            photoMarkupVM.PropertyChanged += PhotoMarkupVM_PropertyChanged;
            this.Markup.DataContext = photoMarkupVM;

            extractionViewVM = new ExtractionInfoVM();
            extractionViewVM.ExtractionName = "Тюменская 168р";
            extractionViewVM.LowerDepth = 2830.0;
            extractionViewVM.UpperDepth = 2800.0;
            this.ExtractionView.DataContext = extractionViewVM;

            
            Activate();
        }

        private void PhotoMarkupVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(photoMarkupVM.CalibratedRegions):
                    extractionViewVM.AnnotatedLength = photoMarkupVM.CalibratedRegions.Sum(r => r.Length) * 1e-2;
                    break;
            }
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
