using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AnnotationPlane
{
    public class AnnotationGrid : System.Windows.Controls.Grid
    {
        public AnnotationGrid()
        {
            Binding b = new Binding("DataContext");
            b.Source = this;
            this.SetBinding(AnnotationGrid.BoundDataContextProperty, b);


            this.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            this.RowDefinitions.Add(new RowDefinition());
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

                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = GridLength.Auto;
                this.ColumnDefinitions.Add(cd);
                
                ColumnVM colVM = e.NewItems[0] as ColumnVM;

                ColumnView view = new ColumnView();
                view.DataContext = colVM;

                Grid.SetColumn(view, this.ColumnDefinitions.Count - 1);
                Grid.SetRow(view, 1);
                this.Children.Add(view);

                RotateTransform headingRotation = new RotateTransform(-90);

                TextBlock heading = new TextBlock();
                heading.Text = colVM.Heading;
                heading.LayoutTransform = headingRotation;

                Grid.SetColumn(heading, this.ColumnDefinitions.Count - 1);
                Grid.SetRow(heading, 0);
                this.Children.Add(heading);




            }
        }
    }
}
