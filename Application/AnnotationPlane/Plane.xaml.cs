using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Xps.Packaging;
using System.Xml;

namespace CoreSampleAnnotation.AnnotationPlane
{
    /// <summary>
    /// Interaction logic for Plane.xaml
    /// </summary>
    public partial class Plane : UserControl
    {
        public Plane()
        {
            InitializeComponent();

            this.AnnoGrid.PointSelected += Plane_PointSelected;
            this.AnnoGrid.ElementDropped += AnnoGrod_ElementDropped;

            DataContextChanged += Plane_DataContextChanged;
        }

        private void AnnoGrod_ElementDropped(object sender, ElemDroppedEventArgs e)
        {
            if (ElementDropped != null)
            {
                ElementDropped.Execute(e);
            }
        }

        private void Plane_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PlaneVM vm = e.NewValue as PlaneVM;
            if (vm != null)
            {
                vm.DragReferenceElem = AnnoGrid.LowerGrid;
                vm.SaveImageCommand = new DelegateCommand(() =>
                {
                    List<Reports.SVG.ISvgRenderableColumn> columnRenderers = new List<Reports.SVG.ISvgRenderableColumn>();
                    int idx = 0;
                    foreach (UIElement elem in AnnoGrid.HeadersGrid.Children) {
                        columnRenderers.Add(new Reports.SVG.ColumnPainter(elem,AnnoGrid.ColumnsGrid.Children[idx] as ColumnView,vm.AnnoGridVM.Columns[idx]));
                        idx++;
                    }
                    var svg = Reports.SVG.Report.Generate(columnRenderers.ToArray());
                    using (XmlTextWriter writer = new XmlTextWriter("result.svg", Encoding.UTF8))
                    {
                        svg.Write(writer);
                    }

                    /*
                    FixedDocument fixedDoc = new FixedDocument();
                    PageContent pageContent = new PageContent();
                    FixedPage fixedPage = new FixedPage();
                    fixedPage.Width = ActualWidth;
                    fixedPage.Height = AnnoGrid.HeadersGrid.ActualHeight + AnnoGrid.ColumnsGrid.ActualHeight;

                    Plane planeToPrint = new Plane();
                    Grid.SetIsSharedSizeScope(planeToPrint, true);
                    //planeToPrint.Width = 800;
                    //planeToPrint.Height = 8000;
                    planeToPrint.DataContext = vm;

                    //Create first page of document
                    fixedPage.Children.Add(planeToPrint);
                    ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                    fixedDoc.Pages.Add(pageContent);
                    //Create any other required pages here

                    //save the document
                    XpsDocument xpsd = new XpsDocument("report.xps", FileAccess.Write);
                    System.Windows.Xps.XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                    xw.Write(fixedDoc);
                    xpsd.Close();
                    */
                });
            }
        }

        public ICommand PointSelected
        {
            get { return (ICommand)GetValue(PointSelectedProperty); }
            set { SetValue(PointSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointSelectedProperty =
            DependencyProperty.Register("PointSelected", typeof(ICommand), typeof(Plane), new PropertyMetadata(null));



        public ICommand ElementDropped
        {
            get { return (ICommand)GetValue(ElementDroppedProperty); }
            set { SetValue(ElementDroppedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ElementDropped.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ElementDroppedProperty =
            DependencyProperty.Register("ElementDropped", typeof(ICommand), typeof(Plane), new PropertyMetadata(null));



        private void Plane_PointSelected(object sender, PointSelectedEventArgs e)
        {
            if (PointSelected != null)
            {
                PointSelected.Execute(e);
            }
        }


    }
}
