using System;
using System.Collections.Generic;
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
    public class PhotoMarkupVM : ViewModel
    {
        private string imagePath = string.Empty;
        public string ImagePath {
            get {                
                    return this.imagePath;
            }
            set {
                if (value != this.imagePath) {
                    this.imagePath = value;
                    this.RaisePropertyChanged(nameof(ImagePath));
                }
            }
        }


    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new PhotoMarkupVM();
            this.Markup.DataContext = vm;
        }
    }
}
