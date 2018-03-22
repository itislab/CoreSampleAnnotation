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
            SetBinding(ClassSelectedCommandProperty, b);

            Binding b2 = new Binding("DataContext.GroupSelectedCommand");
            b2.Source = this;
            SetBinding(GroupSelectedCommandProperty, b2);
        }



        public ICommand ClassSelectedCommand
        {
            get { return (ICommand)GetValue(ClassSelectedCommandProperty); }
            set { SetValue(ClassSelectedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClassSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClassSelectedCommandProperty =
            DependencyProperty.Register("ClassSelectedCommand", typeof(ICommand), typeof(ClassificationView), new PropertyMetadata(null));



        public ICommand GroupSelectedCommand
        {
            get { return (ICommand)GetValue(GroupSelectedCommandProperty); }
            set { SetValue(GroupSelectedCommandProperty, value); }
        }

        public static readonly DependencyProperty GroupSelectedCommandProperty =
            DependencyProperty.Register("GroupSelectedCommand", typeof(ICommand), typeof(ClassificationView), new PropertyMetadata(null));



        private void LeafNode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClassSelectedCommand != null) {
                ClassSelectedCommand.Execute(sender);
            }
        }        

        private void NonLeaf_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GroupSelectedCommand != null)
                GroupSelectedCommand.Execute(sender);
        }
    }

    public class BackgroundColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values != null) && (values.Length == 3))
            {
                LayerClassVM current = values[0] as LayerClassVM;

                bool isSelected = false;

                if (values[1] is LayerClassVM) //selected class
                {
                    if (current == values[1])
                        isSelected = true;
                }
                else if (values[2] is IEnumerable<LayerClassVM>)
                {
                    if (((IEnumerable<LayerClassVM>)values[2]).Contains(current))
                        isSelected = true;
                }

                if (isSelected)
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
            if ((values != null) && (values.Length == 3))
            {                
                LayerClassVM current = values[0] as LayerClassVM;

                bool isSelected = false;

                if (values[1] is LayerClassVM) //selected class
                {
                    if (current == values[1])
                        isSelected = true;
                }
                else if (values[2] is IEnumerable<LayerClassVM>)
                {                    
                    if (((IEnumerable<LayerClassVM>)values[2]).Contains(current))
                        isSelected = true;
                }

                if (isSelected)
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
