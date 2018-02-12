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

namespace CoreSampleAnnotation.AnnotationPlane
{
    /// <summary>
    /// Interaction logic for ClassificationView.xaml
    /// </summary>
    public partial class ClassificationView : UserControl
    {
        public ClassificationView()
        {
            InitializeComponent();

            Binding b = new Binding("DataContext.ClassSelectedCommand");
            b.Source = this;            
            SetBinding(ClassificationView.ClassSelectedCommandProperty, b);
        }



        public ICommand ClassSelectedCommand
        {
            get { return (ICommand)GetValue(ClassSelectedCommandProperty); }
            set { SetValue(ClassSelectedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClassSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClassSelectedCommandProperty =
            DependencyProperty.Register("ClassSelectedCommand", typeof(ICommand), typeof(ClassificationView), new PropertyMetadata(null));

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ClassSelectedCommand != null) {
                ClassSelectedCommand.Execute(sender);
            }
        }
    }

    public class BackgroundColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values != null) && (values.Length == 2))
            {
                LayerClassVM selected = values[0] as LayerClassVM;
                LayerClassVM current = values[1] as LayerClassVM;

                if (selected == current)
                    return new SolidColorBrush(Color.FromRgb(255,61,0));
                else
                    return new SolidColorBrush(Color.FromRgb(93,64,55));
            }
            else return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FontColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values != null) && (values.Length == 2))
            {
                LayerClassVM selected = values[0] as LayerClassVM;
                LayerClassVM current = values[1] as LayerClassVM;

                if (selected == current)
                    return new SolidColorBrush(Color.FromRgb(0,0,0));
                else
                    return new SolidColorBrush(Color.FromRgb(255,255,255));
            }
            else return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
