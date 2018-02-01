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

namespace CoreSampleAnnotation
{
    /// <summary>
    /// Interaction logic for TextInput.xaml
    /// </summary>
    public partial class TextInput : UserControl
    {
        public TextInput()
        {
            InitializeComponent();

            Binding b1 = new Binding(nameof(HintText));
            b1.Source = this;
            HintBlock.SetBinding(TextBlock.TextProperty, b1);

            Binding b2 = new Binding(nameof(Text));
            b2.Mode = BindingMode.TwoWay;
            b2.Source = this;
            b2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            InputBox.SetBinding(TextBox.TextProperty, b2);

            Binding b3 = new Binding(nameof(HintFontSize));
            b3.Source = this;
            HintBlock.SetBinding(TextBlock.FontSizeProperty, b3);

            Binding b4 = new Binding(nameof(InputFontSize));
            b4.Source = this;
            InputBox.SetBinding(TextBox.FontSizeProperty, b4);
        }



        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(TextInput), new PropertyMetadata("Введите значение"));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextInput), new PropertyMetadata(""));



        public int HintFontSize
        {
            get { return (int)GetValue(HintFontSizeProperty); }
            set { SetValue(HintFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintFontSizeProperty =
            DependencyProperty.Register("HintFontSize", typeof(int), typeof(TextInput), new PropertyMetadata(25));



        public int InputFontSize
        {
            get { return (int)GetValue(InputFontSizeProperty); }
            set { SetValue(InputFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputFontsize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputFontSizeProperty =
            DependencyProperty.Register("InputFontSize", typeof(int), typeof(TextInput), new PropertyMetadata(25));






    }
}
