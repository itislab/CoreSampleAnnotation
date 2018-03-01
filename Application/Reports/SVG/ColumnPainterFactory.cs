using CoreSampleAnnotation.AnnotationPlane;
using CoreSampleAnnotation.AnnotationPlane.LayerBoundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSampleAnnotation.Reports.SVG
{
    /// <summary>
    /// Knows about different column types and construct suitable painer for each particular column
    /// </summary>
    public class ColumnPainterFactory
    {
        private static ILayerPainter universalLayerPainter = new LayerPainter();

        public static ColumnPainter Create(UIElement headerView, ColumnView view, ColumnVM vm)
        {
            if (vm is BoundaryEditorColumnVM)
            {
                BoundaryEditorColumnVM becVM = (BoundaryEditorColumnVM)vm;
                ILayerBoundariesVM lbVM = becVM.BoundariesVM;
                if (becVM.ColumnVM is BoundaryLineColumnVM)
                {
                    BoundaryLineColumnVM blcVM = (BoundaryLineColumnVM)becVM.ColumnVM;
                    lbVM = blcVM.BoundariesVM;
                }
                return new BoundaryColumnPainter(headerView, view, vm, lbVM);
            }
            else if (vm is ImageColumnVM)
            {
                return new ImageColumnPainter(headerView, view, (ImageColumnVM)vm);
            }
            else if (vm is DepthAxisColumnVM)
            {
                return new DepthColumnPainter(headerView, view, (DepthAxisColumnVM)vm);
            }
            else if (vm is ILayerColumn)
            {
                return new LayeredColumnPainter(headerView, view, vm, (ILayerColumn)vm, universalLayerPainter);
            }
            else if (vm is SamplesColumnVM) {
                return new SamplesColumnPainter(headerView, view, (SamplesColumnVM)vm);
            }
            else
                return new ColumnPainter(headerView, view, vm);
        }
    }
}
