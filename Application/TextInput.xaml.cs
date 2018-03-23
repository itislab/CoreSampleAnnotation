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

            Binding b2 = new Binding(nameof(Proposal));
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



        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(TextInput), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextInput), new PropertyMetadata("", (depobj,dpca) => {
                //external asignment of text.
                //Propagating it to proposal
                TextInput ti = depobj as TextInput;
                string newValString = dpca.NewValue as string;
                if (newValString != ti.Proposal) {
                    ti.Proposal = newValString;
                }
            }));



        public string Proposal
        {
            get { return (string)GetValue(ProposalProperty); }
            set { SetValue(ProposalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Proposal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProposalProperty =
            DependencyProperty.Register("Proposal", typeof(string), typeof(TextInput), new PropertyMetadata(""));




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



        public Visibility InputConfirmVisibility
        {
            get { return (Visibility)GetValue(InputConfirmVisibilityProperty); }
            set { SetValue(InputConfirmVisibilityProperty, value); }
        }

        public static readonly DependencyProperty InputConfirmVisibilityProperty =
            DependencyProperty.Register("InputConfirmVisibility", typeof(Visibility), typeof(TextInput), new PropertyMetadata(Visibility.Hidden));



        private void InputBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            InputConfirmVisibility = Visibility.Visible;
            BorderBrush = (Brush)FindResource("ColorPrimary");
        }

        private void InputBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //discarding proposal
            Proposal = Text;
            InputConfirmVisibility = Visibility.Hidden;
            BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void FocusToNext() {
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            request.Wrapped = true;
            InputBox.MoveFocus(request);
        }



        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Escape:
                    //descarding proposal
                    Proposal = Text;
                    e.Handled = true;
                    break;
                case Key.Enter:
                    //saving proposal
                    Text = Proposal;
                    e.Handled = true;
                    FocusToNext();
                    break;
                case Key.Tab:
                    //descarding proposal
                    Proposal = Text;
                    e.Handled = true;
                    FocusToNext();
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //saving proposal
            Text = Proposal;
            e.Handled = true;
            FocusToNext();
        }
    }
}
