using Microsoft.Win32;
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

            AnnoGrid.PointSelected += Plane_PointSelected;
            AnnoGrid.ElementDropped += AnnoGrod_ElementDropped;

            Binding scaleBinding = new Binding("DataContext.ScaleFactor");
            scaleBinding.Source = this;
            scaleBinding.Mode = BindingMode.TwoWay;
            AnnoGrid.SetBinding(AnnotationGrid.ScaleFactorProperty, scaleBinding);            

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
                    var dialog = new Microsoft.Win32.SaveFileDialog();
                    dialog.DefaultExt = "svg";
                    dialog.Filter = "Scalable Vector Graphics (.svg)|*.svg";
                    var res = dialog.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        string filepath = dialog.FileName;
                        
                        List<Reports.SVG.ISvgRenderableColumn> columnRenderers = new List<Reports.SVG.ISvgRenderableColumn>();
                        
                        int idx = 0;

                        List<Tuple<Reports.SVG.LegendItemKey, Reports.SVG.ILegendItem>> foundLegendItems = new List<Tuple<Reports.SVG.LegendItemKey, Reports.SVG.ILegendItem>>();

                        foreach (UIElement elem in AnnoGrid.HeadersGrid.Children)
                        {
                            var colVm = vm.AnnoGridVM.Columns[idx];
                            //gathering column SVG representation ...
                            columnRenderers.Add(Reports.SVG.ColumnPainterFactory.Create(elem, AnnoGrid.ColumnsGrid.Children[idx] as ColumnView, colVm));
                            //... and possible appearence in the legend
                            var comparer = new Comparer1();
                            if (colVm is ILayerColumn) {
                                ILayerColumn lcVM = colVm as ILayerColumn;
                                foundLegendItems.AddRange(lcVM.Layers.SelectMany(l => Reports.SVG.LegendFactory.GetLegendItemsForLayer(l)).Distinct(comparer));
                            }
                            if (colVm is Columns.VisualColumnVM) {
                                Columns.VisualColumnVM vcVM = (Columns.VisualColumnVM)colVm;
                                foundLegendItems.AddRange(vcVM.Layers.SelectMany(l => Reports.SVG.LegendFactory.GetLegendItemsForVisualLayer(l)).Distinct(comparer));
                            }
                            idx++;
                        }

                        //the legend items are split into groups
                        Dictionary<Tuple<string, Reports.SVG.PropertyRepresentation>, List<Reports.SVG.ILegendItem>> groupsData
                            = new Dictionary<Tuple<string, Reports.SVG.PropertyRepresentation>, List<Reports.SVG.ILegendItem>>();
                        foreach (var item in foundLegendItems)
                        {
                            Tuple<string, Reports.SVG.PropertyRepresentation> key = Tuple.Create(item.Item1.PropID, item.Item1.Representation);                            
                            if (!groupsData.ContainsKey(key)) {
                                groupsData.Add(key, new List<Reports.SVG.ILegendItem>());
                            }
                            groupsData[key].Add(item.Item2);
                        }

                        //groups are transformed into ILegnedGroups
                        List<Reports.SVG.ILegendGroup> legendGroups = new List<Reports.SVG.ILegendGroup>();
                        foreach (var kvp in groupsData)
                        {
                            Reports.SVG.ILegendGroup group = new Reports.SVG.LegendGroup(
                                kvp.Key.Item1,
                                kvp.Value.ToArray());
                            legendGroups.Add(group);
                        }

                        

                        var svg = Reports.SVG.Report.Generate(columnRenderers.ToArray(), legendGroups.ToArray(),(float)vm.ScaleFactor);
                        using (XmlTextWriter writer = new XmlTextWriter(filepath, Encoding.UTF8))
                        {
                            svg.Write(writer);
                        }

                        MessageBox.Show(string.Format("SVG отчет успешно сохранен в файл {0}", filepath), "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);                        
                    }
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

    class Comparer1 : IEqualityComparer<Tuple<Reports.SVG.LegendItemKey, Reports.SVG.ILegendItem>>
    {
        public bool Equals(Tuple<Reports.SVG.LegendItemKey, Reports.SVG.ILegendItem> x, Tuple<Reports.SVG.LegendItemKey, Reports.SVG.ILegendItem> y)
        {
            return x.Item1.Equals(y.Item1);
        }

        public int GetHashCode(Tuple<Reports.SVG.LegendItemKey, Reports.SVG.ILegendItem> obj)
        {
            return obj.Item1.GetHashCode();
        }
    }
}
