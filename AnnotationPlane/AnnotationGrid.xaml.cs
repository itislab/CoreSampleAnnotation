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

namespace AnnotationPlane
{    
    /// <summary>
    /// Interaction logic for AnnotationGrid.xaml
    /// </summary>
    public partial class AnnotationGrid : UserControl
    {
        public AnnotationGrid()
        {
            InitializeComponent();

            Binding b = new Binding("DataContext");
            b.Source = this;
            this.SetBinding(AnnotationGrid.BoundDataContextProperty, b);            
        }

        public object BoundDataContext
        {
            get { return (object)GetValue(BoundDataContextProperty); }
            set { SetValue(BoundDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoundDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoundDataContextProperty =
            DependencyProperty.Register("BoundDataContext", typeof(object), typeof(AnnotationGrid), new PropertyMetadata(null, AnnotationGrid.DataContexChanged));

        private static void DataContexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnnotationGridVM oldVM = e.NewValue as AnnotationGridVM;
            AnnotationGrid view = (AnnotationGrid)d;
            if (oldVM != null)
            {
                oldVM.Columns.CollectionChanged -= view.Columns_CollectionChanged;
            }

            AnnotationGridVM newVM = e.NewValue as AnnotationGridVM;
            if (newVM != null)
            {
                newVM.Columns.CollectionChanged += view.Columns_CollectionChanged;
            }
        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (e.NewItems.Count != 1)
                    throw new InvalidOperationException();

                ColumnVM colVM = e.NewItems[0] as ColumnVM;

                //handling column itself
                ColumnView view = new ColumnView();
                view.DataContext = colVM;

                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = GridLength.Auto;                
                this.ColumnsGrid.ColumnDefinitions.Add(cd);


                Grid.SetColumn(view, this.ColumnsGrid.ColumnDefinitions.Count - 1);
                Grid.SetRow(view, 1);
                this.ColumnsGrid.Children.Add(view);

                //Handling header
                ColumnDefinition header_cd = new ColumnDefinition();
                header_cd.Width = GridLength.Auto;
                this.HeadersGrid.ColumnDefinitions.Add(header_cd);                                

                RotateTransform headingRotation = new RotateTransform(-90);
                TextBlock heading = new TextBlock();
                heading.Margin = new Thickness(5);
                heading.HorizontalAlignment = HorizontalAlignment.Center;
                heading.VerticalAlignment = VerticalAlignment.Center;
                heading.Text = colVM.Heading;
                heading.LayoutTransform = headingRotation;
                Border textBorder = new Border() { BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(1) };
                textBorder.Child = heading;

                Grid.SetColumn(textBorder, this.HeadersGrid.ColumnDefinitions.Count - 1);
                Grid.SetRow(textBorder, 0);
                this.HeadersGrid.Children.Add(textBorder);

                //hack - syncronizing gric col width by placing dumm objects with bound width to the upper (headers) grid
                Border dummy = new Border();
                Grid.SetColumn(dummy, this.HeadersGrid.ColumnDefinitions.Count - 1);
                Grid.SetRow(dummy, 1);
                this.HeadersGrid.Children.Add(dummy);

                Binding colWidthBinding = new Binding("ActualWidth");
                colWidthBinding.Source = view;                
                header_cd.SetBinding(ColumnDefinition.WidthProperty, colWidthBinding);
            }
        }
    }
}
