using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreSampleAnnotation.AnnotationPlane;
using System.Windows;
using Svg;

namespace CoreSampleAnnotation.Reports.SVG
{
    /// <summary>
    /// Capable of generating SVG for the column header and for the column body
    /// </summary>
    public class ColumnPainter : ISvgRenderableColumn
    {
        protected readonly UIElement headerView;
        protected readonly ColumnVM vm;
        protected readonly ColumnView view;

        public ColumnPainter(UIElement headerView, ColumnView view, ColumnVM vm) {
            this.headerView = headerView;
            this.vm = vm;
            this.view = view;
        }

        public virtual RenderedSvg RenderColumn()
        {
            RenderedSvg result = new RenderedSvg();
            result.RenderedSize = view.RenderSize;
            result.SVG = new Svg.SvgGroup();
            return result;
        }

        public RenderedSvg RenderHeader()
        {
            RenderedSvg result = new RenderedSvg();            
            SvgText text = new SvgText(vm.Heading);
            text.FontSize = new SvgUnit((float)10);
            text.Fill = new SvgColourServer(System.Drawing.Color.Black);
            result.RenderedSize = headerView.RenderSize;
            result.SVG = text;
            return result;
        }

        public virtual SvgDefinitionList Definitions {
            get {
                return new SvgDefinitionList();
            }
        }
    }
}
