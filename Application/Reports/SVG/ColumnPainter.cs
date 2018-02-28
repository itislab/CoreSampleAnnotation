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
    public class ColumnPainter : ISvgRenderableColumn
    {
        protected readonly UIElement headerView;
        protected readonly ColumnVM vm;

        public ColumnPainter(UIElement headerView, ColumnView view, ColumnVM vm) {
            this.headerView = headerView;
            this.vm = vm;
        }

        public RenderedSvg RenderColumn()
        {
            throw new NotImplementedException();
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
    }
}
